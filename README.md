# FlexPath
A flexible C# file/resource path building utility. Never worry about about building and combining paths again!  
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/4f7616c019924c879073fc99b660a24e)](https://app.codacy.com/app/Wortex17/FlexPath?utm_source=github.com&utm_medium=referral&utm_content=Wortex17/FlexPath&utm_campaign=Badge_Grade_Dashboard)
[![GitHub release](https://img.shields.io/github/release/Wortex17/FlexPath)](https://github.com/Wortex17/FlexPath/releases/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/f5tq5q3u4j87a0ux/branch/master?svg=true)](https://ci.appveyor.com/project/Wortex17/flexpath/branch/master)
[![codecov](https://codecov.io/gh/Wortex17/FlexPath/branch/master/graph/badge.svg)](https://codecov.io/gh/Wortex17/FlexPath)  
[![License](https://img.shields.io/github/license/Wortex17/FlexPath)](https://raw.githubusercontent.com/Wortex17/FlexPath/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/Infrablack.FlexPath)](https://www.nuget.org/packages/Infrablack.FlexPath/)

FlexPath allows you to build paths for various platforms, agnostic to the runtime platform it is being executed on.
What sets it apart from .NET tools like ```Path.Combine``` is that FlexPath tokenizes each path before reconstructing it when a combined or collapsed path is required.
This allows FlexPath to retain relative path symbols like ```..``` or ```.``` and enables you to build and collapse actual *relative* paths **without resorting to** filesystem operations like ```Path.GetFullPath```.
In the same manner, FlexPath also helps out with toublesome uncontrolled path input from e.g. command line arguments.
**Parsing Comparison**  
* ```Path.Combine("DirA", "DirB/DirC")```
results in ```DirA\DirB/DirC```
(on windows) because of the sneaky separator in the second argument.  
* ```FlexPath.PathRef.Combine("DirA", "DirB/DirC").WindowsPath``` always results in ```DirA\DirB\DirC``` instead.  
**Collapsing Comparison**  
* ```Path.Combine("DirA", "..", "DirC")```
results in ```DirA\..\DirC```
(on windows) and would require ```Path.GetFullPath()``` which has actual file system communication overhead and always results in an absolute path.  
* ```FlexPath.PathRef.Combine("DirA", "..", "DirC").WindowsPath``` results in the, still possibly relative, ```DirC``` instead.  

## PathRef

FlexPaths functionality is built based on the **PathRef** struct.
A **PathRef** is a simple representation of an absolute or relative path reference.
A **PathRef** can be created with a null, empty or valid string already representing a path
or with static utility methods.

```
PathRef dirBAbsPathRef = new PathRef("DirA/DirB");
PathRef dirARootedAbsPathRef = new PathRef(@"C:\\DirA");
PathRef relativePathRef = new PathRef("../DirB");
PathRef combinedPathRef = PathRef.Combine("DirA", "DirB");
```


### Normalizing
Once a pathRef is built, it can output a normalized path for any supported filesystem, which are currently either
**Windows**, **POSIX** (Unix, Mac) or **LocalPath** for the current runtime default.
```
PathRef pathRef = new PathRef("../DirA/DirB");
Assert.That(pathRef.WindowsPath, Is.EqualTo(@"..\DirA\DirB"));
Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirA/DirB"));
```
Rooted or absolute paths are normalized as you'd expect from ```Path.Combine()``` as well:
```
PathRef pathRef = new PathRef(@"C:\DirA\DirB");
Assert.That(pathRef.WindowsPath, Is.EqualTo(@"C:\DirA\DirB"));
Assert.That(pathRef.PosixPath, Is.EqualTo(@"C:/DirA/DirB"));
```

### Navigation & Joining
A built pathRef enables you to edit it safely via *navigation*, modifying its path to represent the navigation.
Simple navigation modifications can be done via the ```PointToParent``` and ```PointToChild``` methods.
```
PathRef pathRef = new PathRef("DirA/DirB");
pathRef.PointToParent();
Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA"));
pathRef.PointToChild("DirC");
Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA/DirC"));
```
More complex modifications can be done via the ```JoinWith``` method.
```
PathRef pathRef = new PathRef("DirA/DirB");
pathRef.JoinWith("DirE/../DirF"); //Relative paths are also allowed
Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA/DirB/DirF"));
```

### Relative paths
A **PathRef** is relative when it does not start with a base directory but instead begins with ```.``` or ```..```.
```
PathRef pathRef = new PathRef("../DirB");
Assert.That(pathRef.IsRelative, Is.True);
Assert.That(pathRef.IsAbsolute, Is.False);
Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirB"));
```
An absolute path can be made relative via navigation or joining paths that navigate out of the base:
```
PathRef pathRef = new PathRef("DirA/DirB");
pathRef.JoinWith("../../../DirC");
Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirC"));
```

### Clearing paths
A **PathRef** can be emptied out with ```Clear()```
```
PathRef pathRef = new PathRef("DirA/DirB");
pathRef.Clear();
Assert.That(pathRef.PosixPath, Is.EqualTo(""));
```

### Rooting & Unrooting
**PathRef**s can become rooted. This is the case when you set a root segment, like a drive name.
```
PathRef pathRef = new PathRef("C:/DirA");
Assert.That(pathRef.IsRooted, Is.True);
```
A rooted **PathRef** will preserve its root and stay absolute, limiting navigation:
```
pathRef.JoinWith("../../../..");
Assert.That(pathRef.PosixPath, Is.EqualTo(@"C:"));
```
To unroot a **PathRef** it needs to be cleared completely:
```
pathRef.Clear();
Assert.That(pathRef.IsRooted, Is.False);
```

## License
MIT
