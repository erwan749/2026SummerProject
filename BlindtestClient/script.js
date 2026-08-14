let debounceTimer;
let currentVolume = 1;
const searchInput = document.getElementById("searchInput");
const searchResults = document.getElementById("results");


async function searchApi(query) {
   try{
    const reponse =  await(await fetch("https://localhost:7087/api/search?q="+query)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function artistClick(id) {
   try{
    const reponse =  await(await fetch("https://localhost:7087/api/Artists/"+id)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function albumClick(artistId, albumId) {
   try{
    const reponse = await(await fetch(`https://localhost:7087/api/artists/${artistId}/albums/${albumId}`)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function trackClick(artistId, albumId, trackId) {
   try{
    const reponse = await(await fetch(`https://localhost:7087/api/artists/${artistId}/albums/${albumId}/tracks/${trackId}`)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}

function createResultCard(item) {
    const card = document.createElement("div");
    card.classList.add("result-card");
    card.dataset.id = item.id;
    card.dataset.type = item.type;
    card.dataset.artistId = item.artistId;
    card.dataset.albumId = item.albumId;
    card.onclick = handleCardClick;
    card.innerHTML = `<img src="${item.imageUrl}"> <p class="name">${item.name}</p> <p class="subtitle">${item.subtitle ?? ""}</p>`;
    return card;
}

function createArtistPage(artist) {
    const page = document.createElement("div");

    const profile = document.createElement("div");
    page.appendChild(createBackButton());
    profile.classList.add("artist-profile");
    profile.innerHTML = `<img src="${artist.imageUrl}"> <p class="name">${artist.name}</p>`;
    page.appendChild(profile);

    const albumsSection = document.createElement("div");
    albumsSection.classList.add("albums-section");
    albumsSection.innerHTML = `<h3>Albums</h3>`;

    const albumsGrid = document.createElement("div");
    albumsGrid.classList.add("results-grid");
    for (const album of artist.albums) {
        const albumItem = {
            type: "album",
            id: album.id,
            name: album.name,
            imageUrl: album.imageUrl,
            subtitle: "",
            artistId: artist.id,
            albumId: null
        };
        albumsGrid.appendChild(createResultCard(albumItem));
    }
    albumsSection.appendChild(albumsGrid);

    if (artist.hasMoreAlbums) {
        const showMoreBtn = document.createElement("button");
        showMoreBtn.classList.add("show-more-btn");
        showMoreBtn.textContent = "Show more";
        showMoreBtn.onclick = async () => {
            showMoreBtn.disabled = true;
            showMoreBtn.textContent = "Chargement...";
            try {
                const more = await fetch(`https://localhost:7087/api/artists/${artist.id}/albums`).then(r => r.json());
                for (const album of more.albums) {
                    const albumItem = {
                        type: "album",
                        id: album.id,
                        name: album.name,
                        imageUrl: album.imageUrl,
                        subtitle: "",
                        artistId: artist.id,
                        albumId: null
                    };
                    albumsGrid.appendChild(createResultCard(albumItem));
                }
                if (more.hasMoreAlbums) {
                    showMoreBtn.disabled = false;
                    showMoreBtn.textContent = "Show more";
                } else {
                    showMoreBtn.remove();
                }
            } catch (error) {
                console.log(error);
                showMoreBtn.textContent = "Erreur, réessaie";
                showMoreBtn.disabled = false;
            }
        };
        albumsSection.appendChild(showMoreBtn);
    }

    page.appendChild(albumsSection);
    return page;
}

searchInput.addEventListener("input", (event) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => {
    performSearch(event.target.value);
  }, 1000);
});

async function handleCardClick(event) {
    const card = event.currentTarget;

    if (card.dataset.type === "artist") {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const artist = await artistClick(card.dataset.id);
            searchResults.innerHTML = "";
            searchResults.appendChild(createArtistPage(artist));
        } catch (error) {
            console.log(error);
            searchResults.innerHTML = "";
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    }
    else if (card.dataset.type === "album") {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const album = await albumClick(card.dataset.artistId, card.dataset.id);
            searchResults.innerHTML = "";
            searchResults.appendChild(createAlbumPage(album));
        } catch (error) {
            console.log(error);
            searchResults.innerHTML = "";
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    }
    else if (card.dataset.type === "track" && card.dataset.albumId !== "null") {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const album = await albumClick(card.dataset.artistId, card.dataset.albumId);
            const track = await trackClick(card.dataset.artistId, card.dataset.albumId, card.dataset.id);
            searchResults.innerHTML = "";
            searchResults.appendChild(createTrackPlayerPage(track, album.tracks));
        } catch (error) {
            console.log(error);
            searchResults.innerHTML = "";
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    }
}
function formatDuration(seconds) {
    const minutes = Math.floor(seconds / 60);
    const remainingSeconds = seconds % 60;
    const paddedSeconds = remainingSeconds < 10 ? `0${remainingSeconds}` : remainingSeconds;
    return `${minutes}:${paddedSeconds}`;
}

function createAlbumPage(album) {
    const page = document.createElement("div");
    page.appendChild(createBackButton());
    const profile = document.createElement("div");
    profile.classList.add("artist-profile");
    profile.innerHTML = `
        <img src="${album.imageUrl}">
        <p class="name">${album.name}</p>
    `;

    const artistLink = document.createElement("p");
    artistLink.classList.add("subtitle");
    artistLink.style.cursor = "pointer";
    artistLink.textContent = `par ${album.artistName}`;
    artistLink.onclick = async () => {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const artist = await artistClick(album.artistId);
            searchResults.innerHTML = "";
            searchResults.appendChild(createArtistPage(artist));
        } catch (error) {
            console.log(error);
            searchResults.innerHTML = "";
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    };
    profile.appendChild(artistLink);
    page.appendChild(profile);

    const tracksSection = document.createElement("div");
    tracksSection.classList.add("albums-section");
    tracksSection.innerHTML = `<h3>Tracks</h3>`;

    const sortedTracks = [...album.tracks].sort((a, b) => a.trackPosition - b.trackPosition);
    for (const track of sortedTracks) {
        const trackRow = document.createElement("div");
        trackRow.classList.add("result-card");

        trackRow.dataset.type = "track";
        trackRow.dataset.id = track.id;
        trackRow.dataset.artistId = album.artistId;
        trackRow.dataset.albumId = album.id;

        trackRow.onclick = handleCardClick;

        trackRow.innerHTML = `
            <p class="name">${track.trackPosition}. ${track.title}</p>
            <p class="subtitle">${formatDuration(track.duration)}</p>
        `;

        tracksSection.appendChild(trackRow);
    }

    page.appendChild(tracksSection);
    return page;
}
async function performSearch(query) {
    if (query.trim() === "") {
        searchResults.innerHTML = "";
        return;
    }
    try {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        const results = await searchApi(query);
        searchResults.textContent = "";
        if (results.length === 0) {
            searchResults.textContent = "aucun résultat trouvé";
        } else {
            for (const result of results) {
                searchResults.appendChild(createResultCard(result));
            }
        }
    }
    catch (error) {
        console.log(error);
        searchResults.innerHTML = "";
        searchResults.textContent = "Une erreur est survenue, réessaie.";
    }
}

function createBackButton() {
    const backBtn = document.createElement("button");
    backBtn.classList.add("back-button");
    backBtn.textContent = "← Retour à la recherche";
    backBtn.onclick = () => {
        performSearch(searchInput.value);
    };
    return backBtn;
}
function createTrackPlayerPage(track, albumTracks) {
    const page = document.createElement("div");
    page.appendChild(createBackButton());

    const currentIndex = albumTracks.findIndex(t => t.id === track.id);
    const prevTrack = currentIndex > 0 ? albumTracks[currentIndex - 1] : null;
    const nextTrack = currentIndex < albumTracks.length - 1 ? albumTracks[currentIndex + 1] : null;

    const header = document.createElement("div");
    header.classList.add("artist-profile");
    header.innerHTML = `
        <img src="${track.albumImageUrl}" style="width:60px;height:60px;border-radius:8px;">
        <p class="name">${track.title}</p>
    `;
    const artistLink = document.createElement("p");
    artistLink.classList.add("subtitle");
    artistLink.style.cursor = "pointer";
    artistLink.textContent = track.artistName;
    artistLink.onclick = async () => {
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const artist = await artistClick(track.artistId);
            searchResults.innerHTML = "";
            searchResults.appendChild(createArtistPage(artist));
        } catch (error) {
            console.log(error);
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    };
    header.appendChild(artistLink);
    page.appendChild(header);

    const player = document.createElement("div");
    player.classList.add("track-player");

    if (!track.previewUrl) {
        player.innerHTML = `<p class="no-preview">Extrait non disponible pour ce morceau.</p>`;
        page.appendChild(player);
        return page;
    }

    const disc = document.createElement("img");
    disc.classList.add("disc");
    disc.src = track.albumImageUrl;
    player.appendChild(disc);

    const progressBar = document.createElement("div");
    progressBar.classList.add("progress-bar");
    const progressFill = document.createElement("div");
    progressFill.classList.add("progress-fill");
    progressBar.appendChild(progressFill);
    player.appendChild(progressBar);

    const controls = document.createElement("div");
    controls.classList.add("controls");

    const prevBtn = document.createElement("button");
    prevBtn.textContent = "⏮";
    prevBtn.disabled = !prevTrack;

    const playBtn = document.createElement("button");
    playBtn.classList.add("play-btn");
    playBtn.textContent = "▶";

    const nextBtn = document.createElement("button");
    nextBtn.textContent = "⏭";
    nextBtn.disabled = !nextTrack;

    controls.appendChild(prevBtn);
    controls.appendChild(playBtn);
    controls.appendChild(nextBtn);
    player.appendChild(controls);

    const volumeControl = document.createElement("div");
    volumeControl.classList.add("volume-control");
    volumeControl.innerHTML = `🔊`;
    const volumeSlider = document.createElement("input");
    volumeSlider.type = "range";
    volumeSlider.min = "0";
    volumeSlider.max = "1";
    volumeSlider.step = "0.01";
    volumeSlider.value = currentVolume;
    volumeControl.appendChild(volumeSlider);
    player.appendChild(volumeControl);

    const audio = new Audio(track.previewUrl);
    audio.volume = currentVolume;

    playBtn.onclick = () => {
        if (audio.paused) {
            audio.play();
            playBtn.textContent = "⏸";
            disc.classList.add("playing");
        } else {
            audio.pause();
            playBtn.textContent = "▶";
            disc.classList.remove("playing");
        }
    };

    audio.addEventListener("timeupdate", () => {
        if (audio.duration) {
            progressFill.style.width = `${(audio.currentTime / audio.duration) * 100}%`;
        }
    });

    audio.addEventListener("ended", () => {
        playBtn.textContent = "▶";
        disc.classList.remove("playing");
        progressFill.style.width = "0%";
    });

    progressBar.onclick = (event) => {
        const rect = progressBar.getBoundingClientRect();
        const ratio = (event.clientX - rect.left) / rect.width;
        audio.currentTime = ratio * audio.duration;
    };

    volumeSlider.oninput = () => {
        audio.volume = volumeSlider.value;
        currentVolume = volumeSlider.value;
    };

    prevBtn.onclick = async () => {
        audio.pause();
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const newTrack = await trackClick(track.artistId, track.albumId, prevTrack.id);
            searchResults.innerHTML = "";
            searchResults.appendChild(createTrackPlayerPage(newTrack, albumTracks));
        } catch (error) {
            console.log(error);
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    };

    nextBtn.onclick = async () => {
        audio.pause();
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        try {
            const newTrack = await trackClick(track.artistId, track.albumId, nextTrack.id);
            searchResults.innerHTML = "";
            searchResults.appendChild(createTrackPlayerPage(newTrack, albumTracks));
        } catch (error) {
            console.log(error);
            searchResults.textContent = "Une erreur est survenue, réessaie.";
        }
    };

    page.appendChild(player);
    return page;
}