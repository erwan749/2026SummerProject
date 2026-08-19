using System.Collections.Generic;

namespace WebApi.Services
{
    public class AlbumPaginationCache
    {
        private readonly Dictionary<string, string> _nextAlbumsUrlByArtist = new Dictionary<string, string>();

        public void SetNextUrl(string artistId, string nextUrl)
        {
            _nextAlbumsUrlByArtist[artistId] = nextUrl;
        }

        public string GetNextUrl(string artistId)
        {
            return _nextAlbumsUrlByArtist.ContainsKey(artistId) ? _nextAlbumsUrlByArtist[artistId] : null;
        }

        public bool HasMore(string artistId)
        {
            return _nextAlbumsUrlByArtist.ContainsKey(artistId) && _nextAlbumsUrlByArtist[artistId] != null;
        }
    }
}