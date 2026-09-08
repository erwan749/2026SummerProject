import { useState } from 'react'
import Search from './components/Search.jsx'
import ArtistPage from './components/ArtistPage.jsx';
import AlbumPage from './components/AlbumPage.jsx';
import TrackPage from './components/TrackPage.jsx';
import './App.css'

function App() {
  const [selectedArtist , setSelectedArtist] = useState(null);
  const [selectedAlbum , setSelectedAlbum] = useState(null);
  const [selectedTrack , setSelectedTrack] = useState(null);

  let content;
  if (selectedTrack) {
    content = <TrackPage track={selectedTrack} onBack={() => setSelectedTrack(null)}  onTrackClick={(track) => setSelectedTrack(track)}/>
  } else if (selectedAlbum) {
    content = <AlbumPage album={selectedAlbum} onBack={() => setSelectedAlbum(null)} onTrackClick={(track) => setSelectedTrack(track)}/>;
  } else if (selectedArtist) {
    content = <ArtistPage onBack={() => setSelectedArtist(null)} artist={selectedArtist} onAlbumClick={(album) => setSelectedAlbum(album)}/>;
  } else {
    content = <Search onArtistClick={(artist) => setSelectedArtist(artist)} onAlbumClick={(album) => setSelectedAlbum(album) } onTrackClick={(track) => setSelectedTrack(track)}/>;
  }

  return (
    <>
      {content}
    </>
  )
}

export default App
