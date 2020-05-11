using System.Collections.Generic;

[System.Serializable]
public class MyClass
{
    public int myInt;
    public List<int> myList = new List<int>();

    #region CONSTRUCTOR
    public MyClass()
    {

    }

    public MyClass(int myInt, List<int> myList)
    {
        this.myInt = myInt;
        this.myList = myList;
    }
    #endregion

    #region TO STRING
    public override string ToString()
    {
        string output = "";

        output += "myInt = " + myInt + "\n";

        output += "myList = ";
        foreach (int i in myList)
            output += i + ", ";

        return output;
    }
    #endregion
}