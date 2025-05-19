using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Otomasyon.Models
{
    public class TaskItem
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Görev başlığı zorunludur")]
        [Display(Name = "Görev Başlığı")]
        public string Title { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Durum")]
        public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Required]
        public int ProjectId { get; set; }

        [ForeignKey("ProjectId")]
        public Project? Project { get; set; }
    }

    public enum TaskItemStatus
    {
        ToDo,
        InProgress,
        Done
    }
} 