using System.IO;
using NUnit.Framework;

namespace FlexPath.Tests
{
    [TestFixture]
    class PathRefTests
    {
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
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_UsingWindowsPath(string input)
            {
                PathRef pathRef = new PathRef(input);
                Assert.That(pathRef.WindowsPath, Is.EqualTo(pathRef.NormalizePath('\\')));
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
            [TestCase(@"A/B/C/..")]
            [TestCase(@"A/B/C/../..")]
            [TestCase(@"A/B/C/../../..")]
            [TestCase(@"A/B/C/../../../D")]
            [TestCase(@"A/B/C/../../../..")]
            [TestCase(@"A/B/C/../../../../D")]
            [TestCase(@"A/B/C/../../../../D/..")]
            [TestCase(@"A/B/C/../../../../..")]
            public void When_UsingWindowsPath(string input)
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
        }

        [TestCase(@"",@"")]
        [TestCase(null, null)]
        [TestCase(@".", @"")]
        [TestCase(@"./", @"")]
        [TestCase(@"..", @"..")]
        [TestCase(@"../", @"..")]
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
            [TestCase(@"./", @"..")]
            [TestCase(@"..", @"..\..")]
            [TestCase(@"../", @"..\..")]
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
        class Equals
        {
            [TestCase(@"", @"")]
            [TestCase(@"A", @"A")]
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
            [TestCase(@"C:/A", "/A")]
            [TestCase(@"D:/A", "C:/A")]
            [TestCase(@"A/../B", @"/B")]
            public void When_ComparingToInequalPathRef(string inputA, string inputB)
            {
                PathRef pathRefA = new PathRef(inputA);
                PathRef pathRefB = new PathRef(inputB);
                Assert.That(pathRefA.Equals(pathRefB), Is.False);
            }
        }
    }
}
