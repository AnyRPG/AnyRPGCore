public abstract class FishBase : AnimalBase, IFish
{
    public string Tag = default;
}

public interface IFish 
{
    
}
public class RedFish : FishBase
{
    
}  

public class GoldenFish : FishBase
{
    
} 

public class GreenFish : FishBase
{
    
} 
 
public class BlackFish : FishBase
{
    
}