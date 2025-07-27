namespace TechWayFit.Licensing.Management.Core.Models.Common;

public class SemanticVersion
{
    /// <summary>
    /// Gets or sets the major version number.
    /// </summary>
    public int Major { get; set; } = 1;

    /// <summary>
    /// Gets or sets the minor version number.
    /// </summary>
    public int Minor { get; set; } = 0;

    /// <summary>
    /// Gets or sets the patch version number.
    /// </summary>
    public int Patch { get; set; } = 0;

    /// <summary>
    /// Gets or sets the pre-release label (e.g., "alpha", "beta", "rc").
    /// </summary>
    public string PreRelease { get; set; } = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="Version"/> class.
    /// </summary>
    /// <param name="major">The major version number.</param>
    /// <param name="minor">The minor version number.</param>
    /// <param name="patch">The patch version number.</param>
    /// <param name="preRelease">The optional pre-release label.</param>
    public SemanticVersion(int major, int minor, int patch, string preRelease = "")
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        PreRelease = preRelease;
    }

    /// <summary>
    /// Returns the string representation of the version, including pre-release if present.
    /// </summary>
    /// <returns>A string in the format "Major.Minor.Patch[-PreRelease]".</returns>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Patch}" + (string.IsNullOrEmpty(PreRelease) ? "" : $"-{PreRelease}");
    }

    /// <summary>
    /// Parses a version string into a <see cref="SemanticVersion"/> object.
    /// </summary>
    /// <param name="versionString">The version string to parse.</param>
    /// <returns>A <see cref="SemanticVersion"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown if the version string format is invalid.</exception>
    public static SemanticVersion Parse(string versionString)
    {
        var parts = versionString.Split('.');
        if (parts.Length < 3) throw new ArgumentException("Invalid version format");

        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);
        string preRelease = parts.Length > 3 ? parts[3] : string.Empty;

        return new SemanticVersion(major, minor, patch, preRelease);
    }

    /// <summary>
    /// Implicitly converts a <see cref="Version"/> object to its string representation.
    /// </summary>
    /// <param name="version">The <see cref="Version"/> object.</param>
    /// <returns>The string representation of the version.</returns>
    public static implicit operator string(SemanticVersion version)
    {
        return version.ToString();
    }

    /// <summary>
    /// Implicitly converts a version string to a <see cref="Version"/> object.
    /// </summary>
    /// <param name="versionString">The version string.</param>
    /// <returns>A <see cref="Version"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown if the version string format is invalid.</exception>
    public static implicit operator SemanticVersion(string versionString)
    {
        var parts = versionString.Split('.');
        if (parts.Length < 3) throw new ArgumentException("Invalid version format");

        int major = int.Parse(parts[0]);
        int minor = int.Parse(parts[1]);
        int patch = int.Parse(parts[2]);
        string preRelease = parts.Length > 3 ? parts[3] : string.Empty;

        return new SemanticVersion(major, minor, patch, preRelease);
    }

    /// <summary>
    /// Compares two <see cref="SemanticVersion"/> objects to determine if one is greater than the other.
    /// </summary>
    /// <param name="v1">The first <see cref="SemanticVersion"/> object.</param>
    /// <param name="v2">The second <see cref="SemanticVersion"/> object.</param>
    /// <returns><c>true</c> if <paramref name="v1"/> is greater than <paramref name="v2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator >(SemanticVersion v1, SemanticVersion v2)
    {
        if (v1.Major != v2.Major) return v1.Major > v2.Major;
        if (v1.Minor != v2.Minor) return v1.Minor > v2.Minor;
        if (v1.Patch != v2.Patch) return v1.Patch > v2.Patch;
        return string.Compare(v1.PreRelease, v2.PreRelease, StringComparison.Ordinal) > 0;
    }

    /// <summary>
    /// Compares two <see cref="SemanticVersion"/> objects to determine if one is less than the other.
    /// </summary>
    /// <param name="v1">The first <see cref="SemanticVersion"/> object.</param>
    /// <param name="v2">The second <see cref="SemanticVersion"/> object.</param>
    /// <returns><c>true</c> if <paramref name="v1"/> is less than <paramref name="v2"/>; otherwise, <c>false</c>.</returns>
    public static bool operator <(SemanticVersion v1, SemanticVersion v2)
    {
        if (v1.Major != v2.Major) return v1.Major < v2.Major;
        if (v1.Minor != v2.Minor) return v1.Minor < v2.Minor;
        if (v1.Patch != v2.Patch) return v1.Patch < v2.Patch;
        return string.Compare(v1.PreRelease, v2.PreRelease, StringComparison.Ordinal) < 0;
    }

    /// <summary>
    /// Gets the default version (1.0.0).
    /// </summary>
    public static SemanticVersion Default => new SemanticVersion(1, 0, 0);

    public static SemanticVersion Max => new SemanticVersion(99, 99, 99);

    /// <summary>
    /// Gets the latest version (1.0.0-latest).
    /// </summary>
    public static SemanticVersion Latest => new SemanticVersion(1, 0, 0, "latest");

    /// <summary>
    /// Gets the stable version (1.0.0-stable).
    /// </summary>
    public static SemanticVersion Stable => new SemanticVersion(1, 0, 0, "stable");
}