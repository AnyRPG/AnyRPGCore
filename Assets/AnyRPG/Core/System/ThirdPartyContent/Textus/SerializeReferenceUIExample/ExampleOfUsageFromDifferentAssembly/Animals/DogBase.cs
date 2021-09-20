
public abstract class DogBase : MammalBase, IDog
{
    public string Tag = default;
}

public interface IDog 
{ 
    
}

public class RedDog : DogBase
{
    
}  

public class GoldenDog : DogBase
{
     
} 

public class GreenDog : DogBase
{ 
    
} 

public class BlackDog : DogBase
{
    
} 