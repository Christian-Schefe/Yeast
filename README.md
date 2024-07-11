# Yeast: Your Easily Accessible Serialization Toolkit

This Unity package provides a set of utility functions and classes for working with JSON files in Unity projects.

## Features

- Parsing and serializing JSON files

## Installation

To install the package, follow these steps:

1. Open your Unity project.
2. Go to the **Package Manager** window.
3. Click on the **+** button and select **Add package from git URL**.
4. Enter the following URL: `https://github.com/Christian-Schefe/yeast.git`.
5. Click **Add** to install the package.

## Usage

To use Yeast in your project, follow these steps:

1. Import the package into your Unity project.
2. Add the necessary using statements to your script:
   ```csharp
   using Yeast.Json;
   ```
3. Use the provided utility functions and classes to work with JSON in your project.

## Examples

Here are some examples of how to use Yeast:

```csharp
public struct Test {
    public int key;
}
Test obj = Json.Parse<Test>("{\"key\":5}");
string json = Json.Stringify(obj);
Assert.AreEqual("{\"key\":5}", json);
```

## Contributing

Contributions to Yeast are welcome! If you find any issues or have suggestions for improvements, please open an issue or submit a pull request on the [GitHub repository](https://github.com/Christian-Schefe/yeast).

## License

This package is licensed under the [MIT License](https://opensource.org/licenses/MIT).
