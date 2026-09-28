using System.ComponentModel.DataAnnotations;

namespace MediTrack.Data.Models
{
    public class Medicine
    {
        [Key]
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int ReorderLevel { get; set; }
        public decimal UnitPrice { get; set; }
    }
}