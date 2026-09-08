import { useState , useEffect , useRef} from 'react';
import { API_BASE_URL } from '../config';
import './TrackPage.css'


export default function TrackPage({track, onBack, onTrackClick}){
    const [trackDetails,setTrackDetails] = useState(null);
    const [isPlaying,setIsPlaying] = useState(false);
    const [progress, setProgress] = useState(0);
    const [volume, setVolume] = useState(1);
    const audioRef = useRef(null);
    const currentIndex = track.albumTracks ? track.albumTracks.findIndex(t => t.id === track.id) : -1;
    const hasPrevious = track.albumTracks && currentIndex > 0;
    const hasNext = track.albumTracks && currentIndex < track.albumTracks.length - 1;

    useEffect (()=> {
        async function fetchAlbum() {
            const response = await(await fetch(`${API_BASE_URL}/api/artists/${track.artistId}/albums/${track.albumId}/tracks/${track.id}`)).json();
            setTrackDetails(response);
        }
        fetchAlbum();
    },[track.id])
    function togglePlay(){
        if(audioRef.current.paused){
            audioRef.current.play();
            setIsPlaying(true);
        }else{
            audioRef.current.pause();
            setIsPlaying(false);
        }
    }
    function goToTrack(newTrack){
        setProgress(0);
        onTrackClick({...newTrack, artistId : track.artistId , albumId : track.albumId, albumTracks: track.albumTracks})
    }
    useEffect(() => {
        const audio = audioRef.current;
        if (!audio) return;
        function handleEnded() {
            setIsPlaying(false);
        }
        function handleTimeUpdate(){
            setProgress((audio.currentTime / audio.duration) * 100)
        }
        audio.addEventListener("ended", handleEnded);
        audio.addEventListener("timeupdate", handleTimeUpdate);
        return () => {
            audio.removeEventListener("ended", handleEnded);
            audio.removeEventListener("timeupdate", handleTimeUpdate);
        };
    }, [trackDetails]);

    return(
        <>
            <button className="back-button" onClick={onBack}>Back</button>
            {trackDetails ? (
                <div className="track-page">
                    {trackDetails.title}
                    <img className={`disc ${isPlaying ? "playing" : ""}`} src={trackDetails.albumImageUrl} alt={trackDetails.title} />
                    <audio src={trackDetails.previewUrl} ref={audioRef} />
                    {trackDetails.previewUrl ? (
                        <>
                            <div className="progress-bar">
                                <div className="progress-fill" style={{ width: `${progress}%` }}></div>
                            </div>
                            <div className="player-controls">
                            {track.albumTracks && (
                                <button disabled={!hasPrevious} onClick={() => goToTrack(track.albumTracks[currentIndex - 1])}>Précédent</button>
                            )}
                            <button onClick={togglePlay}>{isPlaying ? "⏸" : "▶"}</button>
                            {track.albumTracks && (
                                <button disabled={!hasNext} onClick={() => goToTrack(track.albumTracks[currentIndex + 1])}>Suivant</button>
                            )}
                            </div>
                            <input className="volume-slider" type="range" min="0" max="1" step="0.01" value={volume} onChange={(e) => { const v = e.target.value; setVolume(v); audioRef.current.volume = v; }} />
                        </>
                        ) : "Extrait non disponible"
                    }
                </div>
                ) : (
                <div>Chargement ...</div>
            )}

        </>
    )
}
