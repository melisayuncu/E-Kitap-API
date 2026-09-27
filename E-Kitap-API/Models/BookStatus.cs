namespace E_Kitap_API.Models
{
	// shows tha status of a book
	public enum BookStatus
	{
		Pending, // each new book record starts in the Pending state by default
		Processing, //change the status to Processing when PDF generation begins
		Completed, //done successfully
		Failed //error
	}
}