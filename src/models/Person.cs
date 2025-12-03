namespace TaskOrganizer.Models
{
    public abstract class Person
    {
        private string _firstname = string.Empty;
        private string _lastname = string.Empty;

        public string Firstname
        {
            get => _firstname;
            set => _firstname = value?.Trim() ?? "";
        }


        public string Lastname
        {
            get => _lastname; 
            set => _lastname = value?.Trim() ?? "";
        }

        protected Person(string firstname, string lastname)
        {
            Firstname = firstname;
            Lastname = lastname;
        }  

        protected Person() { } // parameterless constructor for JSON binding
         public virtual string GetFullInfo()
        {
            return $"{Firstname} {Lastname}"; 
        }
    }
}
 