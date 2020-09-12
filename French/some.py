
from System import Console

class Animal:
    def __init__(self, age):
        self.age = age
    def Roar(self):
        Console.WriteLine("My grrrrr age is {0}", age)

class Cat:
    def Roar(self):
        Console.WriteLine("Meow ~~ I'm ~~ {0} years old.", age)

def testCat():
    eva=Cat()
    eva.Roar()

if __name__ == "__main__":
    test()
    