using Microsoft.AspNetCore.Mvc;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Service.Implementations.Task;
using Service.Implementations.User;
using System.Data;

namespace AdminPanel.Helpers.ExcelService
{
	public class ExcelService
	{
		private readonly UserService _userService;
		private readonly TaskService _taskService;

		public ExcelService()
		{
			_userService = new UserService();
			_taskService = new TaskService();
		}

		public IActionResult CreateExcelFile(DataTable result)
		{
			var workbook = new XSSFWorkbook();
			var sheet = workbook.CreateSheet("Sheet1");

			var headerStyle = workbook.CreateCellStyle();

			var customGreen = new XSSFColor(new byte[] { 217, 234, 211 });

			((XSSFCellStyle)headerStyle).SetFillForegroundColor(customGreen);
			headerStyle.FillPattern = FillPattern.SolidForeground;

			var font = workbook.CreateFont();
			font.IsBold = true;
			headerStyle.SetFont(font);

			var header = sheet.CreateRow(0);

			var headers = new[]
			{
				"Görev Adı",
				"İşlem Yapan Kullanıcı",
				"Yapılan İşlem",
				"İşlem Yapılan Görev Anahtar Kelimesi",
				"İşlem Yapılan Kullanıcı",
				"İşlem Yapılan Yorum",
				"İşlem Yapılan Anahtar Kelime",
				"İşlem Tarihi"
			};

			for (int i = 0; i < headers.Length; i++)
			{
				var cell = header.CreateCell(i);
				cell.SetCellValue(headers[i]);
				cell.CellStyle = headerStyle;
			}

			int rowIndex = 1;
			foreach (DataRow dataRow in result.Rows)
			{
				int taskId = dataRow["TaskId"] != DBNull.Value ? Convert.ToInt32(dataRow["TaskId"]) : 0;

				string taskName = "";
				if (taskId > 0)
				{
					var task = _taskService.GetTaskById(taskId);
					taskName = task?.TaskName ?? "";
				}

				int userId = dataRow["UserPerformingTheTransaction"] != DBNull.Value ? Convert.ToInt32(dataRow["UserPerformingTheTransaction"]) : 0;

				string userName = "";
				if (userId > 0)
				{
					var user = _userService.GetUserById(userId);
					userName = user?.FirstName + " " + user?.LastName ?? "";
				}

				var processMap = new Dictionary<int, string>
				{
					{ 0, "Bekliyora çekildi" },
					{ 1, "Devam Ediyora çekildi" },
					{ 2, "Tamamlandı yapıldı" },
					{ 3, "İptal edildi" },
					{ 5, "Kullanıcı eklendi" },
					{ 6, "Kullanıcı çıkarıldı" },
					{ 7, "Anahtar kelime eklendi" },
					{ 8, "Anahtar kelime çıkarıldı" },
					{ 9, "Anahtar kelime tamamlandı" },
					{ 10, "Anahtar kelime devam ediyor" },
					{ 11, "Anahtar kelime oluşturuldu" },
					{ 12, "Anahtar kelime silindi" },
					{ 13, "Yorum yapıldı" },
					{ 14, "Silindi" },
					{ 15, "Eklendi" },
					{ 16, "Güncellendi" }
				};

				int processCode = dataRow["Process"] != DBNull.Value ? Convert.ToInt32(dataRow["Process"]) : -1;
				string processText = processMap.ContainsKey(processCode) ? processMap[processCode] : "Bilinmeyen İşlem";

				int taskTemplateTaskId = dataRow["TaskTemplateTaskId"] != DBNull.Value ? Convert.ToInt32(dataRow["TaskTemplateTaskId"]) : 0;

				string taskTemplateTaskName = "";
				if (taskTemplateTaskId > 0)
				{
					var taskTemplateTask = _taskService.GetTaskTemplateTaskById(taskTemplateTaskId);
					var taskTemplate = _taskService.GetTaskTemplateById(taskTemplateTask.TaskTemplateId);
					taskTemplateTaskName = taskTemplate?.KeyName ?? "";
				}

				int transactionId = dataRow["TransactionUser"] != DBNull.Value ? Convert.ToInt32(dataRow["TransactionUser"]) : 0;

				string transactionName = "";
				if (transactionId > 0)
				{
					var user = _userService.GetUserById(userId);
					transactionName = user?.FirstName + " " + user?.LastName ?? "";
				}

				int commentId = dataRow["TransactionUser"] != DBNull.Value ? Convert.ToInt32(dataRow["TransactionUser"]) : 0;

				string commentName = "";
				if (commentId > 0)
				{
					var comment = _taskService.GetTaskCommentById(commentId);
					commentName = comment?.Comment ?? "";
				}

				int taskTemplateId = dataRow["TaskTemplateId"] != DBNull.Value ? Convert.ToInt32(dataRow["TaskTemplateId"]) : 0;

				string taskTemplateName = "";
				if (taskTemplateId > 0)
				{
					var taskTemplateTask = _taskService.GetTaskTemplateTaskById(taskTemplateId);
					var taskTemplate = _taskService.GetTaskTemplateById(taskTemplateTask.TaskTemplateId);
					taskTemplateName = taskTemplate?.KeyName ?? "";
				}

				var insertedDate = Convert.ToDateTime(dataRow["InsertedDate"]);

				var row = sheet.CreateRow(rowIndex++);
				row.CreateCell(0).SetCellValue(taskName);
				row.CreateCell(1).SetCellValue(userName);
				row.CreateCell(2).SetCellValue(processText);
				row.CreateCell(3).SetCellValue(taskTemplateTaskName);
				row.CreateCell(4).SetCellValue(transactionName);
				row.CreateCell(5).SetCellValue(commentName);
				row.CreateCell(6).SetCellValue(taskTemplateName);
				row.CreateCell(7).SetCellValue(insertedDate.ToString("dd.MM.yyyy HH:mm"));
			}

			using (var ms = new MemoryStream())
			{
				workbook.Write(ms);
				return new FileContentResult(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
				{
					FileDownloadName = "example.xlsx"
				};
			}
		}
	}
}
