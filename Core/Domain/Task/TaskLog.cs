using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Task
{
	public class TaskLog
	{
        public int Id { get; set; }
        public int UserPerformingTheTransaction { get; set; }
        public int? TaskId { get; set; }
        public int Process { get; set; }
        public int? TaskTemplateId { get; set; }
        public int? TaskTemplateTaskId { get; set; }
		public int? TransactionUser { get; set; }
        public int? CommentId { get; set; }
        public DateTime InsertedDate { get; set; }
    }

	//Process
	//
	//0.Bekliyora çekildi
	//1.Devam Ediyora çekildi
	//2.Tamamlandı yapıldı
	//3.İptal edildi
	//5.Kullanıcı eklendi
	//6.Kullanıcı çıkarıldı
	//7.Anahtar kelime eklendi
	//8.Anahtar kelime çıkarıldı
	//9.Anahtar kelime tamamlandı
	//10.Anahtar kelime devam ediyor
	//11.Anahtar kelime oluşturuldu
	//12.Anahtar kelime silindi
	//13.Yorum yapıldı
	//14.Silindi
	//15.Eklendi
	//16.Güncellendi
	//
	//INSERT INTO public."TaskLog"("UserPerformingTheTransaction", "TaskId", "Process", "TemplateId", "TransactionUser", "CommentId") VALUES ( ?, ?, ?, ?, ?, ?);
}
