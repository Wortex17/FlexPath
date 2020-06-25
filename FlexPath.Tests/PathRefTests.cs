using System.IO;
using NUnit.Framework;

namespace FlexPath.Tests
{
    [TestFixture]
    class PathRefTests
    {
        [TestFixture]
        class ReadmeExamples
        {
            [Test]
            public void CombineWithSneakySeparator()
            {
                Assert.That(PathRef.Combine("DirA", "DirB/DirC").WindowsPath, Is.EqualTo(@"DirA\DirB\DirC"));
            }

            [Test]
            public void Normalizing()
            {
                PathRef pathRef = new PathRef("../DirA/DirB");
                Assert.That(pathRef.WindowsPath, Is.EqualTo(@"..\DirA\DirB"));
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirA/DirB"));
            }

            [Test]
            public void NormalizingRooted()
            {
                PathRef pathRef = new PathRef(@"C:\DirA\DirB");
                Assert.That(pathRef.WindowsPath, Is.EqualTo(@"C:\DirA\DirB"));
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"C:/DirA/DirB"));
            }

            [Test]
            public void Navigation()
            {
                PathRef pathRef = new PathRef("DirA/DirB");
                pathRef.PointToParent();
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA"));
                pathRef.PointToChild("DirC");
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA/DirC"));
            }

            [Test]
            public void NavigationJoin()
            {
                PathRef pathRef = new PathRef("DirA/DirB");
                pathRef.JoinWith("DirE/../DirF"); //Relative paths are also allowed
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"DirA/DirB/DirF"));
            }

            [Test]
            public void RelativePaths()
            {
                PathRef pathRef = new PathRef("../DirB");
                Assert.That(pathRef.IsRelative, Is.True);
                Assert.That(pathRef.IsAbsolute, Is.False);
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirB"));
            }

            [Test]
            public void AbsoluteToRelativePaths()
            {
                PathRef pathRef = new PathRef("DirA/DirB");
                pathRef.JoinWith("../../../DirC");
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"../DirC"));
            }

            [Test]
            public void Clearing()
            {
                PathRef pathRef = new PathRef("DirA/DirB");
                pathRef.Clear();
                Assert.That(pathRef.PosixPath, Is.EqualTo(""));
            }

            [Test]
            public void Rooting()
            {
                PathRef pathRef = new PathRef("C:/DirA");
                Assert.That(pathRef.IsRooted, Is.True);
                pathRef.JoinWith("../../../..");
                Assert.That(pathRef.PosixPath, Is.EqualTo(@"C:"));
                pathRef.Clear();
                Assert.That(pathRef.IsRooted, Is.False);
            }
        }

        [TestFixture]
        class NormalizePath
        {
            [TestCase(null, '/', null)]
            [TestCase("", '/', "")]
            [TestCase("A", '/', "A")]
            [TestCase("A/B", '/', "A/B")]
            [TestCase("A/B/C", '/', "A/B/C")]
            [TestCase("A/..", '/', "")]  // Will become empty once resolved
            [TestCase("A/../C", '/', "C")]
            [TestCase("A/../..", '/', "..")]
            [TestCase("C:", '/', "C:")]
            [TestCase("C:/A", '/', "C:/A")]
            [TestCase("C:/A/..", '/', "C:")]
            [TestCase("/", '/', "/")]
            [TestCase("A/", '/', "A/")]
            [TestCase("A/B/", '/', "A/B/")]
            [TestCase("A/B/C/", '/', "A/B/C/")]
            [TestCase("A/../", '/', "./")]
            [TestCase("A/../C/", '/', "C/")]
            [TestCase("A/../../", '/', "../")]
            [TestCase("C:/", '/', "C:/")]
            [TestCase("C:/A/", '/', "C:/A/")]
            [TestCase("C:/A/../", '/', "C:/")]
            public void When_SimplePath(string input, char directorySeparator, string expected)
            {
                PathRef pathRef = new PathRef(input);
                string normalized = pathRef.NormalizePath(directorySeparator);
                Assert.That(normalized, Is.EqualTo(expected));
            }


            [TestCase(".", '/', "")] // Will become empty once resolved
            [TestCase("./", '/', "./")]
            public void When_CurrentDirectory(string input, char directorySeparator, string expected)
            {
                PathRef pathRef = new PathRef(input);
                string normalized = pathRef.NormalizePath(directorySeparator);
                Assert.That(normalized, Is.EqualTo(expected));
            }

            [TestCase("..", '/', "..")]
            [TestCase("../", '/', "../")]
            public void When_ParentDirectory(string input, char directorySeparator, string expected)
            {
                PathRef pathRef = new PathRef(input);
                string normalized = pathRef.NormalizePath(directorySeparator);
                Assert.That(normalized, Is.EqualTo(expected));
            }
        }

        [TestFixture]
        class LocalPath
        {
            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_UsingLocalPath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.LocalPath, Is.EqualTo(pathRef.NormalizePath(Path.DirectorySeparatorChar)));
            }
        }
        
        [TestFixture]
        class WindowsPath
        {
            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_NormalizingAsWindowsPath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.WindowsPath, Is.EqualTo(pathRef.NormalizePath('\\')));
            }

            [TestCase(@"C:\", @"C:\")]
            [TestCase(@"C:\\", @"C:\")]
            [TestCase(@"C:\A", @"C:\A")]
            [TestCase(@"C:\\A", @"C:\A")]
            [TestCase(@"A\B\C\", @"A\B\C\")]
            [TestCase(@"A\B\C", @"A\B\C")]
            [TestCase(@"A\B\C\..", @"A\B")]
            [TestCase(@"\A\B\C\..", @"\A\B")]
            [TestCase(@"\\A\B\C\..", @"\A\B")]
            public void When_Normalizing_FromWindowsBackslashPath(string input, string expectedWindowsPath)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.WindowsPath, Is.EqualTo(expectedWindowsPath));
            }
        }

        [TestFixture]
        class UnixPath
        {
            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_NormalizingAsUnixPath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.PosixPath, Is.EqualTo(pathRef.NormalizePath('/')));
            }
        }

        [TestFixture]
        class Method_ToString
        {
            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_UsingToString(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.ToString(), Is.EqualTo(pathRef.NormalizePath()));
            }
        }

        [TestFixture]
        class StringCasts
        {
            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_FromStringToPath(string input)
            {
                PathRef castedPathRef = input;
                PathRef constructedPathRef = new PathRef(input);
                Assert.That(castedPathRef.ToString(), Is.EqualTo(constructedPathRef.ToString()));
            }

            [TestCase(null)]
            [TestCase(@"")]
            [TestCase(@"C:/")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_FromPathToString(string input)
            {
                PathRef pathRef = new PathRef(input);
                string casted = pathRef;
                Assert.That(casted, Is.EqualTo(pathRef.ToString()));
            }
        }

        [TestFixture]
        class PathModes
        {
            [Test]
            public void When_IsNull()
            {
                PathRef pathRef = new PathRef(null);
                Assert.That(pathRef.IsAbsolute, Is.False);
                Assert.That(pathRef.IsRelative, Is.False);
                Assert.That(pathRef.IsRooted, Is.False);
            }

            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/..")]
            public void When_RelativePathIsGiven(string str)
            {
                PathRef pathRef = new PathRef(str);
                Assert.That(pathRef.IsAbsolute, Is.False);
                Assert.That(pathRef.IsRelative, Is.True);
                Assert.That(pathRef.IsRooted, Is.False);
            }

            [TestCase(@"/A")]
            [TestCase(@"/A/B")]
            [TestCase(@"/A/..")]
            public void When_AbsolutePathIsGiven(string str)
            {
                PathRef pathRef = new PathRef(str);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.IsRelative, Is.False);
                Assert.That(pathRef.IsRooted, Is.False);
            }

            [TestCase(@"C:/")]
            [TestCase(@"C:/A")]
            [TestCase(@"C:/A/B")]
            [TestCase(@"C:/A/B/..")]
            [TestCase(@"http:/")]
            [TestCase(@"http://")]
            [TestCase(@"http://A")]
            public void When_RootedPathIsGiven(string str)
            {
                PathRef pathRef = new PathRef(str);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.IsRelative, Is.False);
                Assert.That(pathRef.IsRooted, Is.True);
            }


            [TestCase(@"C:/")]
            [TestCase(@"A/")]
            [TestCase(@"A/B/")]
            [TestCase(@"A/../")]
            [TestCase(@"./")]
            public void When_PureDirectoryPathIsGiven(string str)
            {
                PathRef pathRef = new PathRef(str);
                Assert.That(pathRef.IsPureDirectory, Is.True);
            }
        }

        [TestCase(@"",@"")]
        [TestCase(null, null)]
        [TestCase(@".", @"")]
        [TestCase(@"./", @".\")]
        [TestCase(@"..", @"..")]
        [TestCase(@"../", @"..\")]
        [TestCase(@"A", @"A")]
        [TestCase(@"A/B", @"A\B")]
        [TestCase(@"A/B/C/..", @"A\B")]
        [TestCase(@"A/B/C/../..", @"A")]
        [TestCase(@"A/B/C/../../..", @"")]
        [TestCase(@"A/B/C/../../../D", @"D")]
        [TestCase(@"A/B/C/../../../..", @"..")]
        [TestCase(@"A/B/C/../../../../D", @"..\D")]
        [TestCase(@"A/B/C/../../../../D/..", @"..")]
        [TestCase(@"A/B/C/../../../../..", @"..\..")]
        [TestCase(@"/", @"\")]
        [TestCase(@"//", @"\")]
        [TestCase(@"///", @"\")]
        [TestCase(@"////", @"\")]
        [TestCase(@"/A", @"\A")]
        [TestCase(@"/A/B", @"\A\B")]
        [TestCase(@"/..", @"\")]
        [TestCase(@"/../..", @"\")]
        [TestCase(@"/A/../..", @"\")]
        [TestCase(@"A/", @"A\")]
        [TestCase(@"A//", @"A\")]
        [TestCase(@"A/../B/", @"B\")]
        public void When_CreatingWithValidInput(string input, string expectedOutput)
        {
            PathRef pathRef = new PathRef(input);
            Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
        }

        [Test]
        public void When_CreatingWithNull()
        {
            PathRef pathRef = new PathRef(null);
            Assert.That(pathRef.IsNull, Is.True);
            Assert.That(pathRef.ToString(), Is.Null);
        }

        [Test]
        public void When_CreatingEmpty()
        {
            PathRef pathRef = new PathRef();
            Assert.That(pathRef.IsNull, Is.False);
            Assert.That(pathRef.IsEmpty, Is.True);
            Assert.That(pathRef.ToString(), Is.EqualTo(""));
        }

        [TestFixture]
        class Clear
        {
            [TestCase(@"")]
            [TestCase(@".")]
            [TestCase(@"./")]
            [TestCase(@"..")]
            [TestCase(@"../")]
            [TestCase(@"../..")]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/B/C")]
            public void When_UsingClearOnRelativePath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(() => { pathRef.Clear(); }, Throws.Nothing);
                Assert.That(pathRef.IsRelative, Is.True);
                Assert.That(pathRef.IsEmpty, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(""));
            }

            [Test]
            public void When_UsingClearOnNullPath()
            {
                PathRef pathRef = new PathRef(null);
                Assert.That(() => { pathRef.Clear(); }, Throws.Nothing);
                Assert.That(pathRef.IsNull, Is.True);
                Assert.That(pathRef.IsEmpty, Is.False);
                Assert.That(pathRef.ToString(), Is.Null);
            }

            [Test]
            public void When_UsingClearOnEmptyPath()
            {
                PathRef pathRef = new PathRef();
                Assert.That(() => { pathRef.Clear(); }, Throws.Nothing);
                Assert.That(pathRef.IsEmpty, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(""));
            }

            [TestCase(@"/")]
            [TestCase(@"/A")]
            [TestCase(@"/A/B")]
            public void When_UsingClearOnAbsolutePath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(() => { pathRef.Clear(); }, Throws.Nothing);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.IsEmpty, Is.False);
                Assert.That(pathRef.ToString(), Is.EqualTo(@"\"));
            }

            [TestCase(@"C:/")]
            [TestCase(@"C:/A")]
            [TestCase(@"http:/A/B")]
            public void When_UsingClearOnRootedPath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(() => { pathRef.Clear(); }, Throws.Nothing);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.IsRooted, Is.False);
                Assert.That(pathRef.IsEmpty, Is.False);
                Assert.That(pathRef.ToString(), Is.EqualTo(@"\"));
            }
        }

        [TestFixture]
        class JoinWith
        {
            [TestCase(@"A", new[]{"B"}, @"A\B")]
            [TestCase(@"A", new[] { "B", "C" }, @"A\B\C")]
            [TestCase(@"A", new[] { ".." }, @"")]
            [TestCase(@"A", new[] { @"..\.." }, @"..")]
            [TestCase(@"A", new[] { "..", ".." }, @"..")]
            [TestCase(@"A", new[] { @"..\..", "..", ".." }, @"..\..\..")]
            public void When_JoiningWithRelativePath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(()=>{pathRef.JoinWith(toJoin);}, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(new[] { "B" }, @"B")]
            public void When_EmptyJoiningWithRelativePath(string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef();
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/..")]
            [TestCase(@"/A")]
            [TestCase(@"/A/B")]
            [TestCase(@"/A/..")]
            public void When_JoiningWithNull(string original)
            {
                PathRef pathRef = new PathRef(original);
                string expectedOutput = pathRef.ToString();
                Assert.That(() => { pathRef.JoinWith(null); }, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }
            
            [Test]
            public void When_EmptyJoiningWithNull()
            {
                PathRef pathRef = new PathRef();
                Assert.That(() => { pathRef.JoinWith(null); }, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(""));
            }

            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"A/..")]
            [TestCase(@"/A")]
            [TestCase(@"/A/B")]
            [TestCase(@"/A/..")]
            public void When_JoiningWithNullArray(string original)
            {
                PathRef pathRef = new PathRef(original);
                string expectedOutput = pathRef.ToString();
                Assert.That(() => { pathRef.JoinWith(new string[]{null, null, null}); }, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"A", new[] { @"\B" }, @"\B")]
            [TestCase(@"A", new[] { @"\B\.." }, @"\")]
            [TestCase(@"A", new[] { @"\B\..\.." }, @"\")]
            [TestCase(@"A", new[] { @"\B", ".." }, @"\")]
            [TestCase(@"A", new[] { @"\B", "C" }, @"\B\C")]
            [TestCase(@"A", new[] { @"\B", "C", ".." }, @"\B")]
            [TestCase(@"A", new[] { @"\B", "..", @"\D" }, @"\D")]
            public void When_JoiningRelativeWithAbsolutePath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"A", new[] { @"C:\B" }, @"C:\B")]
            [TestCase(@"A", new[] { @"C:\B\.." }, @"C:")]
            [TestCase(@"A", new[] { @"C:\B\..\.." }, @"C:")]
            [TestCase(@"A", new[] { @"C:\B", ".." }, @"C:")]
            [TestCase(@"A", new[] { @"C:\B", "C" }, @"C:\B\C")]
            [TestCase(@"A", new[] { @"C:\B", @"D:\B" }, @"D:\B")]
            [TestCase(@"A", new[] { @"C:\B", "C", ".." }, @"C:\B")]
            [TestCase(@"A", new[] { @"C:\B", "..", @"D:\D" }, @"D:\D")]
            public void When_JoiningRelativeWithRootedPath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsRooted, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"\", new[] { @"\B" }, @"\B")]
            [TestCase(@"\A", new[] { @"\B" }, @"\B")]
            [TestCase(@"\A", new[] { @"\" }, @"\")]
            [TestCase(@"\A", new[] { @"\B\.." }, @"\")]
            [TestCase(@"\A", new[] { @"\B", "C" }, @"\B\C")]
            [TestCase(@"\A", new[] { @"\B", @"\C" }, @"\C")]
            public void When_JoiningAbsoluteWithAbsolutePath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"\A", new[] { @"C:\B" }, @"C:\B")]
            [TestCase(@"\A", new[] { @"C:\B\.." }, @"C:")]
            [TestCase(@"\A", new[] { @"C:\B\..\.." }, @"C:")]
            [TestCase(@"\A", new[] { @"C:\B", ".." }, @"C:")]
            [TestCase(@"\A", new[] { @"C:\B", "C" }, @"C:\B\C")]
            [TestCase(@"\A", new[] { @"C:\B", @"D:\B" }, @"D:\B")]
            [TestCase(@"\A", new[] { @"C:\B", "C", ".." }, @"C:\B")]
            [TestCase(@"\A", new[] { @"C:\B", "..", @"D:\D" }, @"D:\D")]
            public void When_JoiningAbsoluteWithRootedPath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsRooted, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"C:", new[] { @"D:" }, @"D:")]
            [TestCase(@"C:\A", new[] { @"C:\B" }, @"C:\B")]
            [TestCase(@"C:\A", new[] { @"C:\B\.." }, @"C:")]
            [TestCase(@"C:\A", new[] { @"C:\B\..\.." }, @"C:")]
            [TestCase(@"C:\A", new[] { @"C:\B", ".." }, @"C:")]
            [TestCase(@"C:\A", new[] { @"C:\B", "C" }, @"C:\B\C")]
            [TestCase(@"C:\A", new[] { @"C:\B", @"D:\B" }, @"D:\B")]
            [TestCase(@"C:\A", new[] { @"C:\B", "C", ".." }, @"C:\B")]
            [TestCase(@"C:\A", new[] { @"C:\B", "..", @"D:\D" }, @"D:\D")]
            public void When_JoiningRootedWithRootedPath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsRooted, Is.True);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"C:", new[] { @"\" }, @"\")]
            [TestCase(@"C:\A", new[] { @"\B" }, @"\B")]
            [TestCase(@"C:\A", new[] { @"\B\.." }, @"\")]
            [TestCase(@"C:\A", new[] { @"\B\..\.." }, @"\")]
            [TestCase(@"C:\A", new[] { @"\B", ".." }, @"\")]
            [TestCase(@"C:\A", new[] { @"\B", "C" }, @"\B\C")]
            [TestCase(@"C:\A", new[] { @"\B", @"\B" }, @"\B")]
            [TestCase(@"C:\A", new[] { @"\B", "C", ".." }, @"\B")]
            [TestCase(@"C:\A", new[] { @"\B", "..", @"\D" }, @"\D")]
            public void When_JoiningRootedWithAbsolutePath(string original, string[] toJoin, string expectedOutput)
            {
                PathRef pathRef = new PathRef(original);
                Assert.That(() => { pathRef.JoinWith(toJoin); }, Throws.Nothing);
                Assert.That(pathRef.IsAbsolute, Is.True);
                Assert.That(pathRef.IsRooted, Is.False);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }
        }

        [TestFixture]
        class PointToParent
        {
            [TestCase(@"", @"..")]
            [TestCase(null, @"..")]
            [TestCase(@".", @"..")]
            [TestCase(@"./", @"..\")]
            [TestCase(@"..", @"..\..")]
            [TestCase(@"../", @"..\..\")]
            [TestCase(@"A", @"")]
            [TestCase(@"A/B", @"A")]
            [TestCase(@"A/B/C", @"A\B")]
            [TestCase(@"A/B/C/..", @"A")]
            [TestCase(@"A/B/C/../..", @"")]
            [TestCase(@"A/B/C/../../..", @"..")]
            [TestCase(@"A/B/C/../../../D", @"")]
            [TestCase(@"A/B/C/../../../..", @"..\..")]
            [TestCase(@"A/B/C/../../../../D", @"..")]
            [TestCase(@"A/B/C/../../../../D/..", @"..\..")]
            [TestCase(@"A/B/C/../../../../..", @"..\..\..")]
            [TestCase(@"/Abs", @"\")]
            [TestCase(@"/", @"\")]
            public void When_UsingPointToParent(string input, string expectedOutput)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(()=>{ pathRef.PointToParent();}, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }
        }

        [TestFixture]
        class PointToChild
        {
            [TestCase(@"", "A", @"A")]
            [TestCase(@".", "A", @"A")]
            [TestCase(@"./", "A", @"A")]
            [TestCase(@"A", "A", @"A\A")]
            [TestCase(@"A", "B", @"A\B")]
            [TestCase(null, "A", "A")]
            [TestCase(@"A/B/C/..", "D", @"A\B\D")]
            [TestCase(@"A/B/C/../..", "D", @"A\D")]
            [TestCase(@"A/B/C/../../..", "D", @"D")]
            [TestCase(@"A/B/C/../../../..", "D", @"..\D")]
            [TestCase(@"A/B/C/../../../../..", "D", @"..\..\D")]
            [TestCase(@"..", "A", @"..\A")]
            [TestCase(@"A/B/C", "D.d", @"A\B\C\D.d")]
            [TestCase(@"/Abs", "D.d", @"\Abs\D.d")]
            public void When_UsingPointToChildWithValidArguments(string input, string childName, string expectedOutput)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(() => { pathRef.PointToChild(childName); }, Throws.Nothing);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }

            [TestCase(@"A/B/C", @".")]
            [TestCase(@"A/B/C", @"..")]
            [TestCase(@"A/B/C", @"\")]
            [TestCase(@"A/B/C", @"/")]
            [TestCase(@"A/B/C", @"a\b")]
            [TestCase(@"A/B/C", @"a/b")]
            public void When_UsingPointToChildWithInvalidArguments(string input, string childName)
            {
                PathRef pathRef = new PathRef(input);
                string expectedOutput = pathRef.ToString();
                Assert.That(() => { pathRef.PointToChild(childName); }, Throws.ArgumentException);
                Assert.That(pathRef.ToString(), Is.EqualTo(expectedOutput));
            }
        }

        [TestFixture]
        class EqualityEquals
        {
            [TestCase(@"", @"")]
            [TestCase(@"A", @"A")]
            [TestCase(@"A/", "A/")]
            [TestCase(@"C:/A", @"C:/A")]
            [TestCase(@"A/B", @"A/B")]
            [TestCase(@"A/../B", @"A/../B")]
            [TestCase(@"A/../B", @"B")]
            [TestCase(@"/A/../B", @"/B")]
            [TestCase(null, null)]
            public void When_ComparingToEqualPathRef(string inputA, string inputB)
            {
                PathRef pathRefA = new PathRef(inputA);
                PathRef pathRefB = new PathRef(inputB);
                Assert.That(pathRefA.Equals(pathRefB), Is.True);
            }

            [Test]
            public void When_ComparingEmptyPathRefs()
            {
                PathRef pathRefA = new PathRef();
                PathRef pathRefB = new PathRef();
                Assert.That(pathRefA.Equals(pathRefB), Is.True);
            }

            [TestCase(@"", @"A")]
            [TestCase(@"", null)]
            [TestCase(null, @"A")]
            [TestCase(@"A", "..")]
            [TestCase(@"A/B", @"A/C")]
            [TestCase(@"/A", "A")]
            [TestCase(@"A/", "A")]
            [TestCase(@"C:/A", "/A")]
            [TestCase(@"D:/A", "C:/A")]
            [TestCase(@"A/../B", @"/B")]
            public void When_ComparingToInequalPathRef(string inputA, string inputB)
            {
                PathRef pathRefA = new PathRef(inputA);
                PathRef pathRefB = new PathRef(inputB);
                Assert.That(pathRefA.Equals(pathRefB), Is.False);
            }

            [TestCase(@"")]
            [TestCase(@"./")]
            [TestCase(@"../")]
            [TestCase(null)]
            [TestCase(@"A")]
            [TestCase(@"A/B")]
            [TestCase(@"/A")]
            [TestCase(@"C:/A")]
            [TestCase(@"A/../B")]
            public void When_ComparingToEmptyPathRef(string inputA)
            {
                PathRef pathRefA = new PathRef(inputA);
                PathRef emptyPathRef = new PathRef();
                Assert.That(pathRefA.Equals(emptyPathRef), Is.False);
            }
        }

        [TestFixture]
        class Static_Combine
        {
            [TestCase(@"", @"")]
            [TestCase(@"", null)]
            [TestCase(null, null)]
            [TestCase(@"", @"A")]
            [TestCase(@"A", @"B")]
            [TestCase(@"A", @"/B")]
            [TestCase(@"A", @"/B")]
            [TestCase(@"A", @"X:/B")]
            [TestCase(@"A", @"../B")]
            [TestCase(@"A", @"./B")]
            [TestCase(@"A/B", @"C")]
            [TestCase(@"A/B", @"../C")]
            public void When_Combine2(string path1, string path2)
            {
                //Take note PathRef.Combine does never initialize the PathRef empty, so we should not expect it to either
                var expectedPathRef = new PathRef(path1);
                expectedPathRef.JoinWith(path2);
                Assert.That(PathRef.Combine(path1, path2), Is.EqualTo(expectedPathRef));
            }

            [TestCase(@"", @"", @"")]
            [TestCase(@"", null, @"")]
            [TestCase(null, null, null)]
            [TestCase(@"", @"A", @"B")]
            [TestCase(@"A", @"B", @"C")]
            [TestCase(@"A", @"/B", @"C")]
            [TestCase(@"A", @"/B", @"/C")]
            [TestCase(@"A", @"/B", @"X:/C")]
            [TestCase(@"A", @"../B", @"C")]
            [TestCase(@"A", @"./B", @"C")]
            public void When_Combine3(string path1, string path2, string path3)
            {
                //Take note PathRef.Combine does never initialize the PathRef empty, so we should not expect it to either
                var expectedPathRef = new PathRef(path1);
                expectedPathRef.JoinWith(path2, path3);
                Assert.That(PathRef.Combine(path1, path2, path3), Is.EqualTo(expectedPathRef));
            }

            [TestCase(@"", @"", @"", @"")]
            [TestCase(@"", null, @"", @"")]
            [TestCase(null, null, null, null)]
            [TestCase(@"", @"A", @"B", @"C")]
            [TestCase(@"A", @"B", @"C", @"D")]
            [TestCase(@"A", @"/B", @"C", "D")]
            [TestCase(@"A", @"/B", @"/C", @"/D")]
            [TestCase(@"A", @"/B", @"X:/C", @"D")]
            [TestCase(@"A", @"../B", @"C", "D")]
            [TestCase(@"A", @"./B", @"C", @"D")]
            public void When_Combine4(string path1, string path2, string path3, string path4)
            {
                //Take note PathRef.Combine does never initialize the PathRef empty, so we should not expect it to either
                var expectedPathRef = new PathRef(path1);
                expectedPathRef.JoinWith(path2, path3, path4);
                Assert.That(PathRef.Combine(path1, path2, path3, path4), Is.EqualTo(expectedPathRef));
            }

            [TestCase(@"", @"", @"", @"", @"")]
            [TestCase(@"", null, @"", @"", @"")]
            [TestCase(null, null, null, null, null)]
            [TestCase(@"", @"A", @"B", @"C", @"D")]
            [TestCase(@"A", @"B", @"C", @"D", @"E")]
            [TestCase(@"A", @"/B", @"C", "D", @"E")]
            [TestCase(@"A", @"/B", @"/C", @"/D", @"/E")]
            [TestCase(@"A", @"/B", @"X:/C", @"D", @"E")]
            [TestCase(@"A", @"../B", @"C", "D", @"E")]
            [TestCase(@"A", @"./B", @"C", @"D", @"E")]
            public void When_Combine5(string path1, string path2, string path3, string path4, string path5)
            {
                //Take note PathRef.Combine does never initialize the PathRef empty, so we should not expect it to either
                var expectedPathRef = new PathRef(path1);
                expectedPathRef.JoinWith(path2, path3, path4, path5);
                Assert.That(PathRef.Combine(path1, path2, path3, path4, path5), Is.EqualTo(expectedPathRef));
            }
        }
    }
}
