import { useState , useEffect} from 'react';
import { API_BASE_URL } from '../config';
import './ArtistPage.css';
import './Search.css';

export default function ArtistPage({artist , onBack ,onAlbumClick}){
    const [artistDetails , setArtistDetails] = useState(null);
    const [loadingMoreAlbums , setLoadingMoreAlbums] = useState(false);
    const [loadError , setLoadError] = useState(false);

    useEffect( () => {
        async function fetchArtist() {
            const  response =  await(await fetch(`${API_BASE_URL}/api/Artists/${artist.id}`)).json();
            setArtistDetails(response);
        }
        fetchArtist()
    }, [artist.id])

    async function loadingMoreAlbumsAsync() {
        setLoadingMoreAlbums(true);
        try{
            const response = await(await fetch(`${API_BASE_URL}/api/artists/${artistDetails.id}/albums`)).json();
            setArtistDetails(prev => ({...prev,albums : [...prev.albums, ...response.albums], hasMoreAlbums: response.hasMoreAlbums}));
            setLoadingMoreAlbums(false);
            setLoadError(false);
        }
        catch(error){
            setLoadError(true);
            setLoadingMoreAlbums(false);
        }
    }

    return(
        <>
            {artistDetails ? <div className="artist-page">
                <button  className="back-button" onClick={onBack}>Back</button>
                <div className='artistContainer'>
                    <img src={artistDetails.imageUrl} alt={artistDetails.name}/>
                    <h2>{artistDetails.name}</h2>
                </div> 
                <div className="results-grid">
                    {artistDetails.albums.map(album => 
                    <div className="result-card" key={album.id} onClick={() => onAlbumClick({ ...album, artistId: artistDetails.id })}>
                        <img src={album.imageUrl} alt={album.name}/>
                        <p>{album.name}</p>
                    </div>)}
                </div>
                {artistDetails.hasMoreAlbums && <button className="show-more-btn" disabled={loadingMoreAlbums} onClick={() =>loadingMoreAlbumsAsync()}>Load more</button>}
                {loadError && <p>Error, try again</p>}
            </div> : <div>Chargement...</div>}
        </>
    )
}
