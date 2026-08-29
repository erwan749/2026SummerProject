let debounceTimer;
let currentVolume = 1;
let selectedTracks = [];
let currentGameSession = null;
let gameResults = [];
let selectedTracksContainerRef = null;

const searchInput = document.getElementById("searchInput");
const searchResults = document.getElementById("results");
const createBlindTestBtn = document.getElementById("createBlindTestBtn"); 
const playBlindTestBtn = document.getElementById("playBlindTestBtn")

const API_BASE_URL = "https://localhost:7087";

let currentAudio = null;


function stopCurrentAudio() {
    if (currentAudio) {
        currentAudio.pause();
        currentAudio = null;
    }
}
async function searchApi(query) {
   try{
    const reponse =  await(await fetch(API_BASE_URL+"/api/search?q="+query)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function searchBlindTestTrack(query){
    try{
        const reponse = await(await fetch(API_BASE_URL+"/api/blindtests/search-tracks?q="+query)).json();
        return reponse;
    }
    catch(error){
        throw error;
    }
}
async function artistClick(id) {
   try{
    const reponse =  await(await fetch(API_BASE_URL+"/api/Artists/"+id)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function albumClick(artistId, albumId) {
   try{
    const reponse = await(await fetch(API_BASE_URL+`/api/artists/${artistId}/albums/${albumId}`)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function trackClick(artistId, albumId, trackId) {
   try{
    const reponse = await(await fetch(API_BASE_URL+`/api/artists/${artistId}/albums/${albumId}/tracks/${trackId}`)).json();
    return reponse;
   }
   catch(error){
    throw error;
   }
}
async function getAllBlindTests(){
    try{
        const reponse = await(await fetch(API_BASE_URL+"/api/blindtests")).json();
        return reponse
    }
    catch(error){
        throw error;
    }
}
async function startGame(blindTestId){
    try{
        const payload = {
            blindTestId : blindTestId
        };
        const reponse = await(await fetch(API_BASE_URL+"/api/Game/start",{
            method : "POST",
            headers :{ "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        })).json();
        return reponse
    }
    catch(error){
        throw(error);
    }
}

async function submitAnswer(sessionId, answerText) {
    try{
        const payload = {
            sessionId : sessionId ,
            answer : answerText
        };
        const reponse = await(await fetch(API_BASE_URL+"/api/Game/answer",{
            method : "POST",
            headers :{ "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        })).json();
        return reponse
    }
    catch(error){
        throw(error);
    }
}

async function getCurrentQuestion(sessionId) {
    try {
        const reponse = await (await fetch(API_BASE_URL + "/api/Game/" + sessionId + "/current")).json();
        return reponse;
    }
    catch (error) {
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
                const more = await fetch(API_BASE_URL+`/api/artists/${artist.id}/albums`).then(r => r.json());
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
    stopCurrentAudio();
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
        searchInput.style.display = "";
        createBlindTestBtn.style.display ="";
        playBlindTestBtn.style.display = "";
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

function createBlindTestPage(){
    const page = document.createElement("div");
    page.appendChild(createBackButton());

    const searchBar = document.createElement("input");
    searchBar.id = "blindTestName";
    searchBar.placeholder = "Nom du blind test";
    page.appendChild(searchBar);

    const blindTestCategory = document.createElement("input");
    blindTestCategory.id = "blindTestCategory";
    blindTestCategory.placeholder = "Catégorie (ex: rap, rock...)";  
    page.appendChild(blindTestCategory);

    const adminKeyInput = document.createElement("input");
    adminKeyInput.id = "adminKeyInput";
    adminKeyInput.type = "password";
    adminKeyInput.placeholder = "Clé admin";
    page.appendChild(adminKeyInput);
    
    const trackSearchInput = document.createElement("input");
    trackSearchInput.id = "trackSearchInput";
    trackSearchInput.placeholder= "Rechercher un titre...";
    page.appendChild(trackSearchInput);

    const trackSearchResults = document.createElement("div");
    trackSearchResults.id = "trackSearchResults";
    trackSearchInput.addEventListener("input" ,(event) => {
            clearTimeout(debounceTimer);
            debounceTimer = setTimeout(() => {
            performTrackSearch(event.target.value , trackSearchResults);
        }, 1000);
    })
    trackSearchResults.classList.add("results-grid");
    page.appendChild(trackSearchResults);

    const selectedTracksContainer = document.createElement("div");
    selectedTracksContainer.id = "selectedTracksContainer";
    page.appendChild(selectedTracksContainer);
    selectedTracksContainerRef = selectedTracksContainer;
    selectedTracksContainer.classList.add("results-grid");
    renderSelectedTracksList(selectedTracksContainer);

    const submitBtn = document.createElement("button");
    submitBtn.classList.add("show-more-btn");
    submitBtn.textContent = "Créer le blind test";
    submitBtn.onclick = async () => {
        if (selectedTracks.length === 0) {
            alert("Sélectionne au moins une track.");
            return;
        }
        submitBtn.disabled = true;
        submitBtn.textContent = "Création en cours...";
        try {
            await createBlindTest(searchBar.value, blindTestCategory.value, adminKeyInput.value, selectedTracks);
            alert("Blind test créé !");
            selectedTracks = [];
            searchInput.style.display = "";
            createBlindTestBtn.style.display = "";
            performSearch(searchInput.value);
        } catch (error) {
            console.log(error);
            alert("Erreur lors de la création.");
            submitBtn.disabled = false;
            submitBtn.textContent = "Créer le blind test";
        }
    };
    page.appendChild(submitBtn);

    return page;
}

async function performTrackSearch(query, container){
    if (query.trim() === "") {
        container.innerHTML = "";
        return;
    }
    try {
        container.innerHTML = "";
        container.textContent = "Recherche en cours...";
        const results = await searchBlindTestTrack(query);
        container.textContent = "";
        if (results.length === 0) {
            container.textContent = "aucun résultat trouvé";
        } else {
            for (const result of results) {
                container.appendChild(createTrackSelectionCard(result));
            }
        }
    }
    catch (error) {
        console.log(error);
        container.innerHTML = "";
        container.textContent = "Une erreur est survenue, réessaie.";
    }
}
createBlindTestBtn.onclick = () => {
    createBlindTestBtn.style.display = "none";
    searchInput.style.display = "none";
    searchResults.innerHTML = "";
    searchResults.appendChild(createBlindTestPage())
};
function createTrackSelectionCard(item) {
    const card = document.createElement("div");
    card.classList.add("result-card");
    card.dataset.id = item.id;
    card.dataset.artistId = item.artistId;
    card.dataset.albumId = item.albumId;

    const checkbox = document.createElement("input");
    checkbox.type = "checkbox";
    checkbox.style.display = "none";
    checkbox.checked = selectedTracks.some(t => t.id === item.id);
    card.classList.toggle("selected", checkbox.checked);

    checkbox.onchange = () => {
        if (checkbox.checked) {
            selectedTracks.push(item);
        } else {
            const confirmed = confirm(`Retirer "${item.name}" de la sélection ?`);
            if (!confirmed) {
                checkbox.checked = true;
                return;
            }
            selectedTracks = selectedTracks.filter(t => t.id !== item.id);
        }
        syncCardsCheckedState(item.id, checkbox.checked);
        if (selectedTracksContainerRef) {
            renderSelectedTracksList(selectedTracksContainerRef);
        }
    };

    card.onclick = () => {
        checkbox.checked = !checkbox.checked;
        checkbox.onchange();
    };

    card.innerHTML = `<img src="${item.imageUrl}"> <p class="name">${item.name}</p> <p class="subtitle">${item.subtitle ?? ""}</p>`;
    card.appendChild(checkbox);
    return card;
}

function renderSelectedTracksList(container) {
    container.innerHTML = "";
    const title = document.createElement("h3");
    title.textContent = `Sélection (${selectedTracks.length})`;
    container.appendChild(title);
    for (const item of selectedTracks) {
        container.appendChild(createTrackSelectionCard(item));
    }
}

async function createBlindTest(name, category, adminKey, tracks) {
    try {
        const payload = {
            name: name,
            category: category,
            adminKey: adminKey,
            tracks: tracks.map(t => ({
                artistId: t.artistId,
                albumId: t.albumId,
                trackId: t.id
            }))
        };
        const response = await fetch(API_BASE_URL+"/api/BlindTests", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });
        const reponse = await response.json();
        return reponse;
    }
    catch (error) {
        throw error;
    }
}

function syncCardsCheckedState(itemId, checked) {
    const trackSearchResults = document.getElementById("trackSearchResults");
    const containers = [trackSearchResults, selectedTracksContainerRef].filter(Boolean);
    containers.forEach(container => {
        container.querySelectorAll(`.result-card[data-id="${itemId}"]`).forEach(card => {
            const cb = card.querySelector('input[type="checkbox"]');
            if (cb) cb.checked = checked;
            card.classList.toggle("selected", checked);
        });
    });
}

async function showBlindTestList(){
    try{
        searchResults.innerHTML = "";
        searchResults.textContent = "Recherche en cours...";
        const results = await getAllBlindTests();
        searchResults.textContent = "";
        const backBtn = createBackButton();
        searchResults.appendChild(backBtn);

        const container = document.createElement("div");
        container.id = "blindTestListContainer";

        for (const result of results){
            const card = document.createElement("div");
            card.classList.add("blindtest-card"); 
            card.innerHTML =`<p class="name">${result.name}</p><p class="category">${result.category}</p>`;
            card.onclick = ()=> startGameFromCard(result.id);;
            container.appendChild(card);
        }
        searchResults.appendChild(container);
    }
    catch(error){
        console.log(error);
        searchResults.innerHTML = "";
        searchResults.textContent = "Une erreur est survenue, réessaie."; 
    }
}
playBlindTestBtn.onclick =() =>{
    createBlindTestBtn.style.display = "none";
    searchInput.style.display = "none";
    playBlindTestBtn.style.display ="none";
    showBlindTestList();
}

async function startGameFromCard(blindTestId){
    currentGameSession = await startGame(blindTestId);
    gameResults = [];
    showGameQuestion();
}
function showGameQuestion() {
    searchResults.innerHTML = "";
    searchResults.appendChild(createBackButton());

    const container = document.createElement("div");
    container.id = "gameQuestionContainer";

    const type = currentGameSession.questionType == "artist" ? "Devine l'artiste" : "Devine le titre";
    container.innerHTML = `<p id="gameProgress">${currentGameSession.questionNumber}/${currentGameSession.totalQuestions}</p><p class="question-type-label">${type}</p>`;

    const canvas = document.createElement("canvas");
    canvas.id = "gameVisualizer";
    canvas.width = 400;
    canvas.height = 120;
    container.appendChild(canvas);

    stopCurrentAudio();
    currentAudio = new Audio(currentGameSession.previewUrl);
    const audio = currentAudio;
    audio.crossOrigin = "anonymous";

    if (!currentGameSession.previewUrl) {
        const noPreview = document.createElement("p");
        noPreview.classList.add("no-preview");
        noPreview.textContent = "Extrait non disponible pour ce morceau.";
        container.appendChild(noPreview);
    } else {
        const playBtn = document.createElement("button");
        playBtn.classList.add("play-btn");
        playBtn.textContent = "▶";
        playBtn.onclick = () => {
            if (audio.paused) {
                audio.play();
                playBtn.textContent = "⏸";
            } else {
                audio.pause();
                playBtn.textContent = "▶";
            }
        };
        container.appendChild(playBtn);
        
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
        container.appendChild(volumeControl);

        audio.volume = currentVolume;

        volumeSlider.oninput = () => {
            audio.volume = volumeSlider.value;
            currentVolume = volumeSlider.value;
        };
        

        audio.addEventListener("ended", () => {
            playBtn.textContent = "▶";
        });
        if (currentGameSession.previewUrl) {
            setupAudioVisualizer(audio, canvas);
        }
        const playPromise = audio.play();
        if (playPromise !== undefined) {
            playPromise.then(() => {
                playBtn.textContent = "⏸";
            }).catch(() => {
                playBtn.textContent = "▶";
            });
        }
    }

    const answerInput = document.createElement("input");
    answerInput.id = "answerInput";
    container.appendChild(answerInput);

    const submitAnswerBtn = document.createElement("button");
    submitAnswerBtn.id = "submitAnswerBtn";
    submitAnswerBtn.textContent = "Valider";
    submitAnswerBtn.onclick = () => handleAnswerSubmit(container, submitAnswerBtn, answerInput);
    container.appendChild(submitAnswerBtn);

    searchResults.appendChild(container);
}

async function handleAnswerSubmit(container, submitAnswerBtn, answerInput) {
    stopCurrentAudio();
    submitAnswerBtn.disabled = true;
    const result = await submitAnswer(currentGameSession.sessionId, answerInput.value);
    gameResults.push(result);

    const feedback = document.createElement("div");
    feedback.id = "answerFeedback";
    feedback.classList.add(result.isCorrect ? "correct" : "incorrect");
    feedback.textContent = result.isCorrect
        ? "Correct !"
        : `Incorrect, la réponse était : ${result.correctAnswer}`;
    container.appendChild(feedback);

    answerInput.disabled = true;

    setTimeout(async () => {
        if (result.isGameOver) {
            showGameSummary();
        } else {
            currentGameSession = await getCurrentQuestion(currentGameSession.sessionId);
            showGameQuestion();
        }
    }, 2000);
}
function showGameSummary() {
    searchResults.innerHTML = "";
    searchResults.appendChild(createBackButton());

    const container = document.createElement("div");
    container.id = "gameSummaryContainer";

    const score = gameResults.filter(r => r.isCorrect).length;
    const total = gameResults.length;

    const scoreEl = document.createElement("p");
    scoreEl.classList.add("game-score");
    scoreEl.textContent = `${score} / ${total}`;
    container.appendChild(scoreEl);

    const list = document.createElement("div");
    list.id = "gameSummaryList";
    list.classList.add("results-grid");

    for (const result of gameResults) {
        const item = {
            type: "track",
            id: result.track.id,
            name: result.track.title,
            imageUrl: result.track.albumImageUrl,
            subtitle: result.track.artistName,
            artistId: result.track.artistId,
            albumId: result.track.albumId
        };
        list.appendChild(createResultCard(item));
    }
    container.appendChild(list);

    searchResults.appendChild(container);
}
function setupAudioVisualizer(audio, canvas) {
    try {
        const audioCtx = new (window.AudioContext || window.webkitAudioContext)();
        const source = audioCtx.createMediaElementSource(audio);
        const analyser = audioCtx.createAnalyser();
        analyser.fftSize = 64;

        source.connect(analyser);
        analyser.connect(audioCtx.destination);

        const ctx = canvas.getContext("2d");
        const bufferLength = analyser.frequencyBinCount;
        const dataArray = new Uint8Array(bufferLength);

        function draw() {
            requestAnimationFrame(draw);
            analyser.getByteFrequencyData(dataArray);

            ctx.clearRect(0, 0, canvas.width, canvas.height);
            const barWidth = (canvas.width / bufferLength) * 1.5;
            let x = 0;

            for (let i = 0; i < bufferLength; i++) {
                const barHeight = (dataArray[i] / 255) * canvas.height;
                ctx.fillStyle = "#00688f";
                ctx.fillRect(x, canvas.height - barHeight, barWidth, barHeight);
                x += barWidth + 2;
            }
        }
        draw();
    } catch (error) {
        console.log("Visualiseur indisponible :", error);
    }
}