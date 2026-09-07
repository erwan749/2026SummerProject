import { useState , useEffect , useRef} from 'react';
import { API_BASE_URL } from '../config';
import './TrackPage.css'


export default function TrackPage({track, onBack}){
    const [trackDetails,setTrackDetails] = useState(null);
    const [isPlaying,setIsPlaying] = useState(false);
    const [progress, setProgress] = useState(0);
    const [volume, setVolume] = useState(1);
    const audioRef = useRef(null);

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
                <div>
                    {trackDetails.title}
                    <audio src={trackDetails.previewUrl} ref={audioRef} />
                    {trackDetails.previewUrl ? (
                        <>
                            <div className="progress-bar">
                                <div className="progress-fill" style={{ width: `${progress}%` }}></div>
                            </div>
                            <button onClick={togglePlay}>{isPlaying ? "⏸" : "▶"}</button>
                            <input type="range" min="0" max="1" step="0.01" value={volume} onChange={(e) => { const v = e.target.value; setVolume(v); audioRef.current.volume = v; }} />
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
