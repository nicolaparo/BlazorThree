using System.Collections;
using System.Numerics;
using System.Reflection;

namespace BlazorThree.Engine;
/// <summary>
/// Represents wire state compactor.
/// </summary>

internal static class WireStateCompactor
{
    private static readonly IReadOnlyDictionary<string, string> Aliases = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        ["animationBlendMs"] = "ab",
        ["animationClipName"] = "ac",
        ["animationLoop"] = "al",
        ["animationSpeed"] = "as",
        ["animationTimeMs"] = "at",
        ["arc"] = "ar",
        ["bevelEnabled"] = "be",
        ["bevelSegments"] = "bg",
        ["bevelSize"] = "bz",
        ["bevelThickness"] = "bt",
        ["boneName"] = "bn",
        ["bonePoses"] = "bp",
        ["camera"] = "ca",
        ["cameraChanged"] = "cc",
        ["capSegments"] = "cg",
        ["className"] = "cn",
        ["click"] = "ck",
        ["closed"] = "cd",
        ["color"] = "cr",
        ["curveSegments"] = "cv",
        ["currentTimeMs"] = "ct",
        ["dampingFactor"] = "df",
        ["depth"] = "d",
        ["detail"] = "dt",
        ["durationMs"] = "du",
        ["easing"] = "ea",
        ["emissive"] = "em",
        ["enableDamping"] = "ed",
        ["enabled"] = "en",
        ["fov"] = "fv",
        ["flatShading"] = "fs",
        ["geometry"] = "g",
        ["gradientMapUrl"] = "gm",
        ["height"] = "h",
        ["heightSegments"] = "hg",
        ["id"] = "i",
        ["indices"] = "ix",
        ["innerRadius"] = "ir",
        ["intensity"] = "it",
        ["interactionChanged"] = "ic",
        ["interactionSubscriptions"] = "ib",
        ["interactionTargets"] = "ig",
        ["ior"] = "io",
        ["isActive"] = "ia",
        ["isAnimationPlaying"] = "ap",
        ["isFull"] = "if",
        ["keyframes"] = "kf",
        ["kind"] = "kd",
        ["length"] = "le",
        ["light"] = "li",
        ["lightChanged"] = "lc",
        ["lightsChanged"] = "lh",
        ["loop"] = "lp",
        ["material"] = "m",
        ["matcapUrl"] = "mu",
        ["meshLambert"] = "ml",
        ["metalness"] = "mt",
        ["mouseEnter"] = "me",
        ["mouseLeave"] = "mv",
        ["name"] = "n",
        ["opacity"] = "op",
        ["openEnded"] = "oe",
        ["orbitControls"] = "oc",
        ["orbitControlsChanged"] = "od",
        ["outerRadius"] = "ou",
        ["outline"] = "ol",
        ["p"] = "pk",
        ["parentId"] = "pi",
        ["pathPoints"] = "pp",
        ["phiLength"] = "pl",
        ["phiSegments"] = "pg",
        ["phiStart"] = "ps",
        ["points"] = "pt",
        ["position"] = "p",
        ["q"] = "qk",
        ["radialSegments"] = "rs",
        ["radius"] = "ra",
        ["radiusBottom"] = "rb",
        ["radiusTop"] = "rt",
        ["reflectivity"] = "rf",
        ["removeGroupIds"] = "rg",
        ["removeLightIds"] = "rl",
        ["removeMeshIds"] = "rm",
        ["removeModelIds"] = "ro",
        ["rotation"] = "r",
        ["roughness"] = "rh",
        ["scale"] = "s",
        ["segments"] = "sg",
        ["shininess"] = "sh",
        ["source"] = "sr",
        ["sourceUrl"] = "su",
        ["specular"] = "sp",
        ["steps"] = "st",
        ["textureUrl"] = "tx",
        ["thetaLength"] = "tl",
        ["thetaSegments"] = "tg",
        ["thetaStart"] = "ts",
        ["thresholdAngle"] = "ta",
        ["timeMs"] = "tm",
        ["timelines"] = "tn",
        ["timelinesChanged"] = "tc",
        ["tracks"] = "tr",
        ["transitions"] = "tt",
        ["transitionsChanged"] = "td",
        ["transmission"] = "te",
        ["tube"] = "tb",
        ["tubularSegments"] = "us",
        ["type"] = "ty",
        ["upsertGroups"] = "ug",
        ["upsertLights"] = "ul",
        ["upsertMeshes"] = "um",
        ["upsertModels"] = "uo",
        ["vertices"] = "vx",
        ["width"] = "w",
        ["widthSegments"] = "wg",
        ["wireframe"] = "wf"
    };

    public static object? Compact(object? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value is string
            || value is bool
            || value is byte
            || value is sbyte
            || value is short
            || value is ushort
            || value is int
            || value is uint
            || value is long
            || value is ulong
            || value is float
            || value is double
            || value is decimal)
        {
            return value;
        }

        if (value is Enum enumValue)
        {
            return enumValue.ToString();
        }

        if (value is Vector3 vector3)
        {
            return new Dictionary<string, object?>(3, StringComparer.Ordinal)
            {
                ["x"] = vector3.X,
                ["y"] = vector3.Y,
                ["z"] = vector3.Z
            };
        }

        if (value is IDictionary dictionary)
        {
            var compactDictionary = new Dictionary<string, object?>(StringComparer.Ordinal);
            foreach (DictionaryEntry entry in dictionary)
            {
                if (entry.Key is not string key)
                {
                    continue;
                }

                compactDictionary[GetAlias(key)] = Compact(entry.Value);
            }

            return compactDictionary;
        }

        if (value is IEnumerable enumerable)
        {
            var items = new List<object?>();
            foreach (var item in enumerable)
            {
                items.Add(Compact(item));
            }

            return items;
        }

        var properties = value.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var compactObject = new Dictionary<string, object?>(properties.Length, StringComparer.Ordinal);

        foreach (var property in properties)
        {
            if (!property.CanRead || property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            compactObject[GetAlias(property.Name)] = Compact(property.GetValue(value));
        }

        return compactObject;
    }

    private static string GetAlias(string propertyName)
    {
        var canonicalName = ToCamelCase(propertyName);
        return Aliases.TryGetValue(canonicalName, out var alias)
            ? alias
            : canonicalName;
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
        {
            return value;
        }

        if (value.Length == 1)
        {
            return char.ToLowerInvariant(value[0]).ToString();
        }

        return string.Create(value.Length, value, static (buffer, source) =>
        {
            buffer[0] = char.ToLowerInvariant(source[0]);
            source.AsSpan(1).CopyTo(buffer[1..]);
        });
    }
}
