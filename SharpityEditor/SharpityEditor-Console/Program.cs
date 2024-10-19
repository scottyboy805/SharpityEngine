// See https://aka.ms/new-console-template for more information
using SharpityEditor;

Console.WriteLine("Hello, World!");


using (GameEditor editor = new GameEditor())
{
    editor.OpenProject("TestProject/TestProject.json");
}