# LabelImg
yolo label tools

python 3.10+

pip install fastapi ultralytics python-multipart uvicorn

1. find task pid:
netstat -ano | findstr :5000
TCP    127.0.0.1:5000         0.0.0.0:0              LISTENING       10088

2. kill task
taskkill /pid 10088 /F

or find & kill task
for /f "tokens=5" %a in ('netstat -ano ^| findstr :5000') do taskkill /PID %a /F