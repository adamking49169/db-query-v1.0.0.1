using CsvHelper.Configuration;
using db_query_v1._0._0._1.Models;

namespace db_query_v1._0._0._1.Data
{
    public sealed class PassengerMap : ClassMap<Passenger>
    {
        public PassengerMap()
        {
            try
            {
                Map(m => m.PassengerId).Name("PassengerId");
                Map(m => m.Survived).Name("Survived");
                Map(m => m.Pclass).Name("Pclass");
                Map(m => m.Name).Name("Name");
                Map(m => m.Sex).Name("Sex");
                Map(m => m.Age).Name("Age").TypeConverterOption.NullValues("NULL", "").Default(0);  // Handling missing or "NULL" values for Age
                Map(m => m.SibSp).Name("SibSp");
                Map(m => m.Parch).Name("Parch");
                Map(m => m.Ticket).Name("Ticket");
                Map(m => m.Fare).Name("Fare").TypeConverterOption.NullValues("NULL", "").Default(0m);  // Handling missing or "NULL" values for Fare
                Map(m => m.Cabin).Name("Cabin");
                Map(m => m.Embarked).Name("Embarked");
            }
            catch (Exception e)
            {
                var b = e.ToString();
               
            }
           
        }
    }
}
