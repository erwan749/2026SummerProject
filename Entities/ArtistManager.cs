using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    public class ArtistManager
    {
        public List<Artist> Artists { get; private set; } = new List<Artist>();

        private Artist GetArtistByExternalId(int externalId , string error)
        {
            if (!Artists.Any(a => a.ExternalId == externalId)) throw new Exception(error);
            return Artists.First(a => a.ExternalId == externalId);
        }


        public void Add(Artist artist)
        {
            if (artist == null) throw new ArgumentNullException();


            if (Artists.Any(a => a.ExternalId == artist.ExternalId)) throw new Exception("Artist is already in the list");

            Artists.Add(artist); 
        }

        public void Remove(Artist artist) {

            if (artist == null) throw new ArgumentNullException();
            Artists.Remove(GetArtistByExternalId(artist.ExternalId , "Artist is not in the list"));

        }
        public void Update(Artist artist)
        {
            if (artist == null) throw new ArgumentNullException();

            Artist existingArtist = GetArtistByExternalId(artist.ExternalId, "Artist is not in the list");

            int artistIndex = Artists.FindIndex(a => a.ExternalId == existingArtist.ExternalId);

            Artists[artistIndex] = artist;
        }
        public void AddAlbum (int externalId, Album album)
        {
            if (album == null) throw new ArgumentNullException();
            Artist artist = GetArtistByExternalId(externalId, "Artist is not in the list");
            album.Artist= artist;
            artist.Albums.Add(album);
        }
        public void AddTrack (int externalId, int albumId, Track track)
        {
            if (track == null) throw new ArgumentNullException(null,"Track is null");
            Artist artist = GetArtistByExternalId(externalId, "Artist does not exist");
            if (!artist.Albums.Any(a => a.Id == albumId)) throw new Exception("Album does not exist");
            int albumIndex = artist.Albums.FindIndex(a => a.Id == albumId);
            artist.Albums[albumIndex].Tracks.Add(track);
            track.Album = artist.Albums.First(a => a.Id == albumId);
        }
        public Track RemoveTrack(int externalId, int albumId, Track track)
        {
            if (track == null) throw new ArgumentNullException(null,"Track is null");

            if (!Artists.Any(a => a.ExternalId == externalId))
                throw new ArgumentNullException(null, "Artist does not exist");

            Artist artist = Artists.First(a => a.ExternalId == externalId);

            int albumIndex = artist.Albums.FindIndex(a => a.Id == albumId);

            if (albumIndex == -1) throw new ArgumentNullException(null,"Album does not exist");

            int trackIndex = artist.Albums[albumIndex].Tracks.FindIndex(t => t.Id == track.Id);

            if (trackIndex == -1) throw new Exception("Track does not exist in the album");

            Track existingTrack = artist.Albums[albumIndex].Tracks[trackIndex];

            artist.Albums[albumIndex].Tracks.Remove(existingTrack);

            existingTrack.Album = null;

            return existingTrack;
        }
    }
}
