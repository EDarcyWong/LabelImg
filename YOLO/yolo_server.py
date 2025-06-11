from fastapi import FastAPI, File, UploadFile, Form
from fastapi.responses import JSONResponse
from ultralytics import YOLO
from PIL import Image
import io
import logging
import os
import requests
import json
from tqdm import tqdm
import sys


# 配置日志
logging.basicConfig(level=logging.INFO, format="%(asctime)s [%(levelname)s] %(message)s")
logger = logging.getLogger(__name__)

app = FastAPI()
model = None
current_model_path = None

MODELS_JSON = "../models.json"

# 自动下载模型的函数
def download_model(url: str, save_path: str):
    logger.info(f"Downloading model from {url} to {save_path}")
    response = requests.get(url, stream=True)
    if response.status_code != 200:
        raise Exception(f"HTTP {response.status_code}: Failed to download from {url}")
    
    total = int(response.headers.get('content-length', 0))
    with open(save_path, 'wb') as file, tqdm(
        desc=os.path.basename(save_path),
        total=total,
        unit='iB',
        unit_scale=True,
        unit_divisor=1024,
        file=sys.stdout,  # <-- 强制输出到标准输出
        ascii=True
    ) as bar:
        for data in response.iter_content(chunk_size=1024):
            size = file.write(data)
            bar.update(size)
    logger.info(f"Model downloaded: {save_path}")

# 根据文件名从 models.json 查找下载链接
def find_download_url(model_filename: str) -> str:
    if not os.path.exists(MODELS_JSON):
        raise FileNotFoundError(f"{MODELS_JSON} file not found.")

    with open(MODELS_JSON, "r", encoding="utf-8") as f:
        models = json.load(f)

    for item in models:
        if item.get("FileName") == model_filename:
            return item.get("DownloadUrl")

    raise FileNotFoundError(f"Download URL for {model_filename} not found in {MODELS_JSON}.")


# 加载模型函数
def load_model(model_path: str):
    global model, current_model_path

    # 如果模型不存在，则尝试从远程下载
    if not os.path.isfile(model_path):
        os.makedirs(os.path.dirname(model_path), exist_ok=True)
        model_filename = os.path.basename(model_path)
        try:
            download_url = find_download_url(model_filename)
            download_model(download_url, model_path)
        except Exception as e:
            raise FileNotFoundError(f"Model not found locally and failed to download: {str(e)}")


    model = YOLO(model_path)
    current_model_path = model_path
    logger.info(f"Model loaded: {model_path}")

# 启动时加载默认模型
default_model = "models/yolo11x.pt"
load_model(default_model)

@app.post("/load_model")
async def switch_model(model_path: str = Form(...)):
    try:
        load_model(model_path)
        return {"status": "success", "message": f"Model switched to {model_path}"}
    except Exception as e:
        logger.error(str(e))
        return JSONResponse(content={"status": "error", "message": str(e)}, status_code=400)

@app.post("/detect")
async def detect(file: UploadFile = File(...)):
    if model is None:
        return JSONResponse(content={"error": "Model not loaded."}, status_code=500)

    contents = await file.read()
    image = Image.open(io.BytesIO(contents)).convert("RGB")
    width, height = image.size
    logger.info(f"Received image: {file.filename}, size: {width}x{height}")

    results = model(image)

    detections = []
    for result in results:
        boxes = result.boxes
        logger.info(f"Detected {len(boxes)} objects.")
        for box in boxes:
            x1, y1, x2, y2 = box.xyxy[0].cpu().numpy().tolist()
            conf = float(box.conf[0].cpu().numpy())
            cls = int(box.cls[0].cpu().numpy())
            label = model.names[cls]

            # 中心点
            cx = (x1 + x2) / 2
            cy = (y1 + y2) / 2
            
            # 获取图像尺寸用于归一化
            width, height = image.size
            x1_norm = x1 / width
            y1_norm = y1 / height
            x2_norm = x2 / width
            y2_norm = y2 / height
            cx_norm = cx / width
            cy_norm = cy / height
            
            detections.append({
                "bbox": [
                    round(float(x1), 6),
                    round(float(y1), 6),
                    round(float(x2), 6),
                    round(float(y2), 6)
                ],
                "center": [
                    round(float(cx), 6),
                    round(float(cy), 6)
                ],
                "bbox_norm": [
                    round(float(x1_norm), 6),
                    round(float(y1_norm), 6),
                    round(float(x2_norm), 6),
                    round(float(y2_norm), 6)
                ],
                "weight":[
                    round(float(x2_norm - x1_norm), 6),
                    round(float(y2_norm - y1_norm), 6)
                ],
                "center_norm": [
                    round(float(cx_norm), 6),
                    round(float(cy_norm), 6)
                ],
                "confidence": round(float(conf), 6),
                "class_id": int(cls),
                "label": label
            })
            logger.info(f"bbox {int(cls), [ round(float(x1_norm), 6), round(float(y1_norm), 6), round(float(x2_norm), 6), round(float(y2_norm), 6)]}")


    return JSONResponse(content={"detections": detections})

if __name__ == "__main__":
    import uvicorn
    uvicorn.run("yolo_server:app", host="127.0.0.1", port=5000, reload=False)
