using System;


namespace db_query_v1._0._0._1.Models
{
    public class Passenger
    {
        public int PassengerId { get; set; }
        public int Survived { get; set; }
        public int Pclass { get; set; }
        public string Name { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }  // Nullable to handle missing values
        public int SibSp { get; set; }
        public int Parch { get; set; }
        public string Ticket { get; set; }
        public decimal? Fare { get; set; }  // Nullable to handle missing values
        public string Cabin { get; set; }
        public string Embarked { get; set; }

        // Constructor for easy initialization
        public Passenger(int passengerId, int survived, int pclass, string name, string sex, double? age, int sibSp, int parch, string ticket, double fare, string cabin, string embarked)
        {
            PassengerId = passengerId;
            Survived = survived;
            Pclass = pclass;
            Name = name;
            Sex = sex;
            Age = (int?)age;
            SibSp = sibSp;
            Parch = parch;
            Ticket = ticket;
            Fare = (decimal?)fare;
            Cabin = cabin;
            Embarked = embarked;
        }

        // Optionally, you can add ToString method to display the information nicely
        public override string ToString()
        {
            return $"{PassengerId}, {Survived}, {Pclass}, {Name}, {Sex}, {Age}, {SibSp}, {Parch}, {Ticket}, {Fare}, {Cabin}, {Embarked}";
        }
    }

}
