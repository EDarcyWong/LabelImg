using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace LabelImg.YOLO
{
    public class YoloTool
    {
        private readonly InferenceSession session;
        private readonly int inputSize = 640;
        private readonly string[] classNames = new[] {
            "car"
        };
        //private readonly string[] classNames = new[] {
        //    "person", "bicycle", "car", "motorcycle", "airplane", "bus", "train",
        //    "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter",
        //    "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear",
        //    "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase",
        //    "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat",
        //    "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle",
        //    "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple",
        //    "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut",
        //    "cake", "chair", "couch", "potted plant", "bed", "dining table", "toilet",
        //    "tv", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave",
        //    "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase",
        //    "scissors", "teddy bear", "hair drier", "toothbrush"
        //};

        public YoloTool(string onnxPath)
        {
            session = new InferenceSession(onnxPath);
        }

        public Bitmap DetectImage(string imagePath)
        {
            using var original = new Bitmap(imagePath);
            using var resized = new Bitmap(original, new Size(inputSize, inputSize));
            var tensor = ExtractTensorFromImage(resized);

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", tensor)
            };

            using var results = session.Run(inputs);
            var output = results.First().AsEnumerable<float>().ToArray();

            var detections = ParseOutputs(output, original.Width, original.Height);

            Bitmap outputImage = new Bitmap(original);
            using (var g = Graphics.FromImage(outputImage))
            using (var font = new Font("Arial", 12))
            {
                foreach (var det in detections)
                {
                    g.DrawRectangle(Pens.Red, det.Box);
                    g.DrawString($"{det.Label} {det.Confidence:0.00}", font, Brushes.Red, det.Box.X, det.Box.Y - 15);
                }
            }

            return outputImage;
        }

        private DenseTensor<float> ExtractTensorFromImage(Bitmap bitmap)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 3, inputSize, inputSize });

            for (int y = 0; y < inputSize; y++)
            {
                for (int x = 0; x < inputSize; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);
                    // RGB 顺序
                    tensor[0, 0, y, x] = pixel.R / 255f;
                    tensor[0, 1, y, x] = pixel.G / 255f;
                    tensor[0, 2, y, x] = pixel.B / 255f;
                }
            }

            return tensor;
        }

        private class Detection
        {
            public RectangleF Box { get; set; }
            public string Label { get; set; }
            public float Confidence { get; set; }
        }

        private List<Detection> ParseOutputs(float[] output, int origW, int origH)
        {
            var result = new List<Detection>();
            int rows = output.Length / 84;

            float scaleX = origW / (float)inputSize;
            float scaleY = origH / (float)inputSize;

            for (int i = 0; i < rows; i++)
            {
                float objConf = output[i * 84 + 4];
                if (objConf < 0.3f)
                    continue;

                // 找出最高的 class score
                float maxClassScore = 0f;
                int classId = -1;
                for (int j = 5; j < 84; j++)
                {
                    if (output[i * 84 + j] > maxClassScore)
                    {
                        maxClassScore = output[i * 84 + j];
                        classId = j - 5;
                    }
                }

                float conf = objConf * maxClassScore;
                if (conf < 0.3f) continue;

                float cx = output[i * 84 + 0];
                float cy = output[i * 84 + 1];
                float w = output[i * 84 + 2];
                float h = output[i * 84 + 3];

                float x = (cx - w / 2) * scaleX;
                float y = (cy - h / 2) * scaleY;
                float boxW = w * scaleX;
                float boxH = h * scaleY;

                result.Add(new Detection
                {
                    Box = new RectangleF(x, y, boxW, boxH),
                    Label = classId >= 0 && classId < classNames.Length ? classNames[classId] : $"Class {classId}",
                    Confidence = conf
                });
            }

            return result;
        }
    }
}
