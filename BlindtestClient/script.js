let debounceTimer;

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

function createResultCard(item) {
    const card = document.createElement("div");
    card.classList.add("result-card");
    card.innerHTML = `<img src="${item.imageUrl}"> <p class="name">${item.name}</p> <p class="subtitle">${item.subtitle ?? ""}</p>`;
    return card;
}

searchInput.addEventListener("input", (event) => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(async () => {
    if (event.target.value.trim() === "") {
        searchResults.innerHTML = "";
        return;
    }
    try{
        searchResults.innerHTML = "";
        searchResults.textContent="Recherche en cours..."
        const results = await searchApi(event.target.value);
        searchResults.textContent=""
        if (results.length === 0){
            searchResults.textContent="aucun résultat trouvé"
        }else{
            for (const result of results) {
                searchResults.appendChild(createResultCard(result));
            }
        }
    }
    catch(error){
        console.log(error);
        searchResults.innerHTML = "";
        searchResults.textContent = "Une erreur est survenue, réessaie.";
    }
  }, 1000);
});