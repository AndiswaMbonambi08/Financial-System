using Financial_System.Models;
using Financial_System.Services;
using Financial_System.Data;

namespace Financial_System.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
