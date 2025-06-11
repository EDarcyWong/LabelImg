using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace LabelImg.YOLO
{
    public class YoloClient
    {
        private readonly HttpClient httpClient;
        private readonly string baseUrl;

        public YoloClient(string baseUrl = "http://localhost:5000")
        {
            this.baseUrl = baseUrl.TrimEnd('/');
            httpClient = new HttpClient();
        }

        public async Task<string> DetectAsync(string imagePath)
        {
            if (!File.Exists(imagePath))
                throw new FileNotFoundException("图像文件不存在", imagePath);

            using var content = new MultipartFormDataContent();
            var imageContent = new ByteArrayContent(File.ReadAllBytes(imagePath));
            imageContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            content.Add(imageContent, "file", Path.GetFileName(imagePath)); // 必须为 file 字段

            var response = await httpClient.PostAsync($"{baseUrl}/detect", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return json;
        }

        public async Task<DetectionResult[]> DetectAndParseAsync(string imagePath)
        {
            var json = await DetectAsync(imagePath);
            var result = JsonSerializer.Deserialize<DetectionResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return result?.detections ?? Array.Empty<DetectionResult>();
		}

		// 新增：调用 /load_model 接口，动态切换模型
		public async Task<bool> LoadModelAsync(string modelPath)
		{
			using var content = new MultipartFormDataContent();
			content.Add(new StringContent(modelPath), "model_path");

			var response = await httpClient.PostAsync($"{baseUrl}/load_model", content);
			if (response.IsSuccessStatusCode)
			{
				return true;
			}
			else
			{
				var errMsg = await response.Content.ReadAsStringAsync();
				throw new Exception($"加载模型失败: {errMsg}");
			}
		}
	}

    public class DetectionResponse
    {
        public DetectionResult[] detections { get; set; }
    }

    public class DetectionResult
    {
        public string label { get; set; }
        public float confidence { get; set; }
        public float[] bbox { get; set; }           // [x1, y1, x2, y2]
        public float[] center { get; set; }         // 🔺 新增字段 [cx, cy]
        public float[] bbox_norm { get; set; }      // 🔺 新增字段 [x1_norm, y1_norm, x2_norm, y2_norm]
        public float[] weight { get; set; }
        public float[] center_norm { get; set; }    // 🔺 新增字段 [cx_norm, cy_norm]
        public int class_id { get; set; }           // 🔺 新增字段
    }
}
