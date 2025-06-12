# LabelImg
yolo label tools

## Environment
### OS: 
Windows 11

### Dev Tool: 
Visual Studio 2022

### Yolo Env:
python 3.10+

pip
#### packages
pip install fastapi ultralytics python-multipart uvicorn

---

## Clear Python Task
1. find task pid:
netstat -ano | findstr :5000
TCP    127.0.0.1:5000         0.0.0.0:0              LISTENING       10088

2. kill task
taskkill /pid 10088 /F

or find & kill task
for /f "tokens=5" %a in ('netstat -ano ^| findstr :5000') do taskkill /PID %a /F