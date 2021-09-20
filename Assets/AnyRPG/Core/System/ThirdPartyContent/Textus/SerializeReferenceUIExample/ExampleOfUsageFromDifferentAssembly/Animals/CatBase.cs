
public abstract class CatBase : MammalBase, ICat
{
    public string Tag = default;
}
 
public interface ICat 
{
    
}

public class RedCat : CatBase
{
    
} 

public class GoldenCat : CatBase
{
     
} 

public class GreenCat : CatBase
{
    
} 
 
public class BlackCat : CatBase
{
    
} 