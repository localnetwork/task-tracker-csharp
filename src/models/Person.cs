namespace TaskOrganizer.Models
{
    // Abstraction: Person defines a general concept 
    public abstract class Person
    {
        private string _firstname;
        private string _lastname;

        public string Firstname 
        {
            get => _firstname; 
            protected set
            {
                // if (string.IsNullOrWhiteSpace(value))
                //     throw new ArgumentException("Firstname cannot be empty");
                _firstname = value.Trim();
            }
        }

        public string Lastname
        { 
            get => _lastname;
            protected set
            {
                // if (string.IsNullOrWhiteSpace(value))
                //     throw new ArgumentException("Lastname cannot be empty");
                _lastname = value.Trim();
            } 
        }

        // private int _age; 
        // public int Age
        // {
        //     get => _age;
        //     protected set
        //     {
        //         if (value < 0 || value > 120)
        //             throw new ArgumentException("Invalid age.");
        //         _age = value;
        //     }
        // }
 
         protected Person(string firstname, string lastname)
        { 
            Firstname = firstname;
            Lastname = lastname;
        }

        // Polymorphism
        public virtual string GetFullInfo()
        {
            return $"{Firstname} {Lastname}";
        }
    }
}
