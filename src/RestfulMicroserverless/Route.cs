using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace RestfulMicroseverless
{
    internal class Route
    {
        private readonly IList<RouteSegment> _routeSegments;

        public Route(string pathTemplate)
        {
            PathTemplate = pathTemplate;
            _routeSegments = PathTemplate.Split('/').Select(segment => new RouteSegment(segment)).ToList();
        }

        public string PathTemplate { get; }

        public override string ToString()
        {
            return PathTemplate;
        }

        public bool Matches(string invokedPath)
        {
            var invokedSegments = invokedPath.Split('/').ToList();
            if (_routeSegments.Count != invokedSegments.Count) return false;
            using (var routeSegmentsEnumerator = _routeSegments.GetEnumerator())
            using (var invokedSegmentsEnumerator = invokedSegments.GetEnumerator())
            {
                while (routeSegmentsEnumerator.MoveNext() && invokedSegmentsEnumerator.MoveNext())
                    if (!routeSegmentsEnumerator.Current.Matches(invokedSegmentsEnumerator.Current)) return false;
            }
            return true;
        }

        public IDictionary<string, string> BuildPathParameters(string invokedPath)
        {
            var pathParameterDictionary = new Dictionary<string, string>();
            var invokedSegments = invokedPath.Split('/').ToList();
            using (var routeSegmentsEnumerator = _routeSegments.GetEnumerator())
            using (var invokedSegmentsEnumerator = invokedSegments.GetEnumerator())
            {
                while (routeSegmentsEnumerator.MoveNext() && invokedSegmentsEnumerator.MoveNext())
                {
                    var currentSegment = routeSegmentsEnumerator.Current;
                    if (currentSegment.IsPathParameter) pathParameterDictionary.Add(currentSegment.PathParameterKey, invokedSegmentsEnumerator.Current);
                }
            }
            return pathParameterDictionary;
        }

        public class RouteSegment
        {
            private const string _validCharactersInPathSegment = "^[A-Za-z0-9\\-._~\\\\:/?#\\[\\]@!$&'\\(\\)\\*\\+,;=`.]+$";
            private static readonly Regex _pathParameterRegex = new Regex("^\\{([A-Za-z0-9]+)\\}$");
            private readonly string _segment;
            private string _pathParameterKey;
            private Regex _segmentPattern;

            public RouteSegment(string segment)
            {
                _segment = segment;
                BuildSegmentRegex();
            }

            public bool IsPathParameter { get; private set; }

            public string PathParameterKey
            {
                get
                {
                    if (!IsPathParameter) throw new ArgumentException("Cannot get the PathParameter key if the RouteSegment is not a PathParameter.");
                    return _pathParameterKey;
                }
            }

            public bool Matches(string invokedSegment)
            {
                return _segmentPattern.IsMatch(invokedSegment);
            }

            private void BuildSegmentRegex()
            {
                var match = _pathParameterRegex.Match(_segment);
                if (match.Success)
                {
                    IsPathParameter = match.Success;
                    _segmentPattern = new Regex(_validCharactersInPathSegment);
                    _pathParameterKey = match.Groups[1].Value;
                }
                else
                {
                    _segmentPattern = new Regex($"^{_segment}$");
                }
            }
        }
    }
}