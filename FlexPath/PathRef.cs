using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FlexPath
{
    /// <summary>
    /// A simple representation of a relative or absolute path reference.
    /// </summary>
    /// <remarks>
    /// Made to be a sensible replacement of <see cref="Path.Combine(string,string)"/>
    /// and a lightweight but flexible replacement of <see cref="Uri"/>
    /// It supports normalizing as well as joining relative path references to a condensed relative path,
    /// without having to fall back to a absolute path.
    /// Joining this path reference with an absolute reference, makes this an absolute reference as well.
    /// </remarks>
    public struct PathRef : IEquatable<PathRef>
    {
        /// <summary>
        /// Segment denoting a navigation to the parent directory
        /// </summary>
        const string ParentDirectorySegment = "..";
        /// <summary>
        /// Segment denoting a path relative to the current workspace (or another reference path)
        /// </summary>
        const string CurrentDirectorySegment = ".";

        /// <summary>
        /// Similar to <see cref="Path.Combine(string,string)"/>, combines strings assuming them to be path references.
        /// </summary>
        public static PathRef Combine(string path1, string path2)
        {
            var pathRef = new PathRef(path1);
            pathRef.JoinWith(path2);
            return pathRef;
        }

        /// <summary>
        /// Similar to <see cref="Path.Combine(string,string,string)"/>, combines strings assuming them to be path references.
        /// </summary>
        public static PathRef Combine(string path1, string path2, string path3)
        {
            var pathRef = Combine(path1, path2);
            pathRef.JoinWith(path3);
            return pathRef;
        }

        /// <summary>
        /// Similar to <see cref="Path.Combine(string,string,string,string)"/>, combines strings assuming them to be path references.
        /// </summary>
        public static PathRef Combine(string path1, string path2, string path3, string path4)
        {
            var pathRef = Combine(path1, path2, path3);
            pathRef.JoinWith(path4);
            return pathRef;
        }

        /// <summary>
        /// Similar to <see cref="Path.Combine(string[])"/>, combines strings assuming them to be path references.
        /// </summary>
        public static PathRef Combine(params string[] paths)
        {
            var pathRef = new PathRef(null);
            pathRef.JoinWith(paths);
            return pathRef;
        }

        public bool IsNull => m_IsNull;

        public bool IsEmpty => !IsNull && !IsAbsolute && !HasAnyChildren && !HasAnyParents;

        /// <summary>
        /// A Path is a pure directory path if it contains a trailing directory separator
        /// </summary>
        public bool IsPureDirectory => m_IsDirectory;

        /// <summary>
        /// A Path is absolute if it either it <see cref="IsRooted"/> or if it was built/joined with an absolute reference and therefore should contain
        /// a prefix to make it absolute too.
        /// </summary>
        public bool IsAbsolute => !IsNull && IsRooted || m_IsAbsolute;

        public bool IsRelative => !IsNull && !IsAbsolute;

        /// <summary>
        /// Once this flexPath is rooted, it cannot be unrooted again.
        /// In rooted mode, if discards path segments that would lead beyond the root, as those are considered impossible.
        /// </summary>
        public bool IsRooted => !IsNull && m_RootSegment != null;

        /// <summary>
        /// Default constructor, takes a single path which serves as a anchorPath for building the reference relative upon
        /// </summary>
        public PathRef(string anchorPath)
        {
            m_Children = new SegmentedPathUnsafe();
            m_Parents = 0;
            m_IsNull = anchorPath == null;
            m_IsAbsolute = false;
            m_RootSegment = null;
            m_IsDirectory = false;
            JoinWith(anchorPath);
        }

        /// <summary>
        /// Joins this path with a group of other paths. Paths will be joined sequentially, similar to how <see cref="Path.Combine(string,string)"/> works.
        /// If the other path is relative, it will be resolved against this path to result in a condensed relative reference or update the absolute reference.
        /// If the other path is absolute, it will replace this path with the absolute reference.
        /// </summary>
        public void JoinWith(params string[] paths)
        {
            if (paths == null)
            {
                return;
            }

            foreach (var path in paths)
            {
                JoinWith(path);
            }
        }

        /// <summary>
        /// Joins this path with another path.
        /// If the other path is relative, it will be resolved against this path to result in a condensed relative reference or update the absolute reference.
        /// If the other path is absolute, it will replace this path with the absolute reference.
        /// </summary>
        void JoinWith(string path)
        {
            if (path == null)
            {
                return;
            }

            m_IsNull = false;
            var newSegments = path.Split(new[]
            {
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            });

            for (int i = 0; i < newSegments.Length; i++)
            {
                JoinWithSegment(newSegments[i], i == 0, i == newSegments.Length-1);
            }
        }

        void JoinWithSegment(string segment, bool isStartSegment, bool isEndSegment)
        {
            //New segment indicates previous trailing directory separator is not trailing
            m_IsDirectory = false;
            if (String.IsNullOrEmpty(segment))
            {
                if (isStartSegment && !isEndSegment)
                {
                    //Empty start segment is a path that started with a separator - an absolute path reference
                    //A new absolute reference replaces any relative or absolute reference that was set before
                    Clear();
                    m_IsAbsolute = true;
                    return;
                }
                else if(isEndSegment && !isStartSegment)
                {
                    //Empty end segment is cut off
                    m_IsDirectory = true;
                    return;
                }
                else
                {
                    //Empty segments are skipped
                    return;
                }
            }

            if (isStartSegment && segment.EndsWith(Path.VolumeSeparatorChar.ToString()))
            {
                //Start segment with volume separator at the end means it was a root or protocol
                //A new root reference replaces any relative or absolute reference that was set before
                Clear();
                m_RootSegment = segment;
                return;
            }

            switch (segment)
            {
                case ParentDirectorySegment:
                    PointToParent();
                    break;
                case CurrentDirectorySegment:
                    //Nothing to do here
                    break;
                default:
                    PointToChildUnsafe(segment);
                    break;
            }
        }

        /// <summary>
        /// Changes the path to point to the parent directory of the currently referenced target.
        /// </summary>
        public void PointToParent()
        {
            m_IsNull = false;
            if (m_Children.HasAny)
            {
                m_Children.Pop();
            }
            else if (IsAbsolute)
            {
                //Cannot go beyond root / absolute depth, discard this PointToParent
            }
            else
            {
                m_Parents++;
            }
        }

        /// <summary>
        /// Changes the path to point to the child directory or file of the currently referenced target.
        /// </summary>
        public void PointToChild(string childName)
        {
            //We only disallow the bare minimum for valid path specification - we do not check for full filesystem path validity

            int errorIndex = childName.IndexOf("/", StringComparison.Ordinal);
            if (errorIndex >= 0)
            {
                throw new ArgumentException($"PointToChild childName '{childName}' contains invalid character '/' at index {errorIndex}");
            }

            errorIndex = childName.IndexOf(@"\", StringComparison.Ordinal);
            if (errorIndex >= 0)
            {
                throw new ArgumentException($"PointToChild childName '{childName}' contains invalid character '\\' at index {errorIndex}");
            }

            if (childName.Trim(new[] {'.'}).Length == 0)
            {
                throw new ArgumentException($"PointToChild childName '{childName}' contains only dots which is invalid");
            }
            PointToChildUnsafe(childName);
        }

        /// <summary>
        /// Clears out the reference, making it an empty path.
        /// It will retain <see cref="IsAbsolute"/> but not <see cref="IsRooted"/>
        /// </summary>
        public void Clear()
        {
            m_IsAbsolute = IsAbsolute;
            m_RootSegment = null;
            m_Children.Clear();
            m_IsDirectory = false;
            m_Parents = 0;
        }

        private void PointToChildUnsafe(string childName)
        {
            m_IsDirectory = false;
            m_IsNull = false;
            m_Children.Push(childName);
        }

        /// <summary>
        /// Path, as it should look like in a Windows file system
        /// </summary>
        public string WindowsPath => NormalizePath('\\');
        /// <summary>
        /// Path, as it should look like in a Unix file system
        /// </summary>
        public string PosixPath => NormalizePath('/');
        /// <summary>
        /// Path as it should look like on the local file system
        /// </summary>
        public string LocalPath => NormalizePath();

        /// <summary>
        /// Normalizes the path using the systems <see cref="Path.DirectorySeparatorChar"/>.
        /// Appends a trailing directory separator if this path is <see cref="IsPureDirectory"/>
        /// </summary>
        public string NormalizePath()
        {
            return NormalizePath(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Normalizes the path using the custom provided <see cref="directorySeparator"/>
        /// Appends a trailing directory separator if this path is <see cref="IsPureDirectory"/>
        /// </summary>
        public string NormalizePath(char directorySeparator)
        {
            return NormalizePath(directorySeparator, IsPureDirectory);
        }

        /// <summary>
        /// Normalizes the path using the custom provided <see cref="directorySeparator"/>
        /// Overrides the appending of a trailing directory separator.
        /// Note that it will not be appended to root-only paths.
        /// Note that appending a trailing separator may add a <see cref="CurrentDirectorySegment"/> if the path is empty
        /// </summary>
        public string NormalizePath(char directorySeparator, bool appendTrailingSeparator)
        {
            if (IsNull)
            {
                return null;
            }

            if (IsEmpty)
            {
                if (appendTrailingSeparator)
                {
                    return CurrentDirectorySegment + directorySeparator;
                }
                return string.Empty;
            }

            SharedStringBuilder.Clear();

            bool isInitialSegment = true;
            if (IsRooted)
            {
                SharedStringBuilder.Append(m_RootSegment);
                isInitialSegment = false;
            }
            else if (IsAbsolute)
            {
                SharedStringBuilder.Append(directorySeparator);
            }

            int addedSegments = 0;
            for (int i = 0; i < m_Parents; i++)
            {
                if (!isInitialSegment)
                {
                    SharedStringBuilder.Append(directorySeparator);
                }

                isInitialSegment = false;
                SharedStringBuilder.Append(ParentDirectorySegment);
                addedSegments++;
            }

            m_Children.IterateSegments((string segment) =>
            {
                if (!isInitialSegment)
                {
                    SharedStringBuilder.Append(directorySeparator);
                }

                isInitialSegment = false;
                SharedStringBuilder.Append(segment);
                addedSegments++;
            });

            if (appendTrailingSeparator && (addedSegments > 0 || IsRooted))
            {
                SharedStringBuilder.Append(directorySeparator);
            }

            string normalizedPath = SharedStringBuilder.ToString();
            return normalizedPath;
        }

        public override string ToString()
        {
            return NormalizePath();
        }

        public bool Equals(PathRef other)
        {
            bool isEqual =
                    object.Equals(m_IsNull, other.m_IsNull) &&
                    object.Equals(m_IsAbsolute, other.m_IsAbsolute) &&
                    object.Equals(m_Parents, other.m_Parents) &&
                    object.Equals(m_RootSegment, other.m_RootSegment) &&
                    object.Equals(m_IsDirectory, other.m_IsDirectory) &&
                    object.Equals(m_Children, other.m_Children)
                ;
            return isEqual;
        }

        public static implicit operator string(PathRef pathRef) => pathRef.ToString();
        public static implicit operator PathRef(string path) => new PathRef(path);

        private bool HasAnyChildren => m_Children.HasAny;
        private bool HasAnyParents => m_Parents > 0;

        private bool m_IsNull;
        /// <summary>
        /// If true, this path is considered absolute.
        /// Absolute non-rooted paths should be prefixed by the directorySeparator
        /// </summary>
        private bool m_IsAbsolute;
        /// <summary>
        /// The segment to prefix if defined. Marks this path as rooted.
        /// </summary>
        private string m_RootSegment;

        /// <summary>
        /// The number of parents that have to be visited before visiting the children.
        /// </summary>
        private int m_Parents;
        /// <summary>
        /// The encoded array of children that have to be visited.
        /// </summary>
        private SegmentedPathUnsafe m_Children;
        /// <summary>
        /// True if this path contains a trailing directory separator.
        /// </summary>
        private bool m_IsDirectory;

        private static readonly StringBuilder SharedStringBuilder = new StringBuilder();
    }
}
