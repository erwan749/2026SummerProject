import { useState , useEffect} from 'react';
import { API_BASE_URL } from '../config';
import './AlbumPage.css';

export default function AlbumPage({album , onBack, onTrackClick}){
    const [albumDetails , setAlbumDetails] = useState(null);

    useEffect (()=> {
        async function fetchAlbum() {
            const response = await(await fetch(`${API_BASE_URL}/api/artists/${album.artistId}/albums/${album.id}`)).json();
            setAlbumDetails(response);
        }
        fetchAlbum();
    },[album.id])

    return(
        <>
            {albumDetails ? <div>

                <button className="back-button" onClick={onBack}>Back</button>
                <div className='albumContainer'>
                    <div className='detailedAlbum'>
                        <img src={albumDetails.imageUrl} alt={albumDetails.name}/>
                        <h2 className='albumTitle'> {albumDetails.name} </h2>
                        <p>by {albumDetails.artistName}</p>
                    </div>
                    <p>TRACKS</p>
                    <div className='albumTrackContainer'>
                        {albumDetails.tracks.map(track => 
                        <div className='albumTrack' key={track.id} onClick={ () => onTrackClick({...track, artistId: albumDetails.artistId, albumId : albumDetails.id})}>
                            <p>{track.title}</p>
                        </div>)}
                    </div>
                </div>

            </div>
            
            : <div>Chargement...</div>}
        </>
    )
}