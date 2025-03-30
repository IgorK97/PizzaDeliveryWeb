public class LoginModel
{
    public string UserName { get; set; }
    public string Password { get; set; }
    //Ролей у одного пользователя может быть несколько, поэтому при входе нужно е1 указать
    public string SelectedRole { get; set; }
}