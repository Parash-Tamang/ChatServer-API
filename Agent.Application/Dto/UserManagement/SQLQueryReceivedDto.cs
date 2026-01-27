namespace Agent.Application.Dto.UserManagement
{
    public class SQLQueryReceivedDto
    {
        public string sqlQuery { get; set; }
        public string status { get; set; }

        public int statusCode { get; set; }
        public string Message { get; set; }

    }
}
