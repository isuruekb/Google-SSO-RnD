namespace SSO.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }


    public class EfassObj
    {
        public int Id { get; set; }
        public string Name { get; set; }
      
    }
}