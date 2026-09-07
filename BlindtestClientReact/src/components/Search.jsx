import { useState, useEffect } from 'react';
import { API_BASE_URL } from '../config';
import './Search.css';

export default function Search({ onArtistClick , onAlbumClick,onTrackClick }) {
  const [query, setQuery] = useState("");
  const [results, setResults] = useState([]);

  useEffect(() => {
    if (query === ""){setResults([]); return;}
    const timer = setTimeout(async () => {
      const response = await (await fetch(`${API_BASE_URL}/api/search?q=${query}`)).json();
      setResults(response);
    } , 1000);
    return () => {clearTimeout(timer)};
  } , [query]);

  return(
    <div className="search-page">
        <input placeholder="Rechercher un artiste, un album, un titre..." onChange={(e) => setQuery(e.target.value)} value={query}/>
        <div className="results-grid">
          {results.map(result => <div className="result-card" key={result.id} onClick={() => {if(result.type === "artist") onArtistClick(result);if(result.type === "album") onAlbumClick(result);if (result.type === "track" && result.albumId !== null) onTrackClick(result);}}>
            <img src={result.imageUrl} alt={result.name}/>
            <p className='name'>{result.name}</p>
            <span className='subtitle'>{result.subtitle}</span>
          </div>)}
        </div>
        
    </div>
  )
}