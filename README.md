# BlindTest - Implémentation personnelle

## Contexte
Projet réalisé dans le cadre du projet d'été des DIIAGE.
Ce dépôt contient uniquement mon implémentation personnelle réalisée pendant ce projet.
Les énoncés, ressources pédagogiques et tests fournis par l'école ne sont pas inclus dans ce dépôt.

## Description du projet
Application de blind test musical : gestion d'entités représentant des artistes, des albums et
des morceaux, une API permettant de rechercher et naviguer dans ce contenu via l'API Spotify
(avec un fallback sur l'API Deezer pour les extraits audio manquants), et un client web permettant
d'effectuer cette recherche, de naviguer entre artiste/album/morceau, et d'écouter les extraits.

## Fonctionnalités implémentées

### Modèle de données (Entities)
Gestion en mémoire des artistes, albums et morceaux, avec relations bidirectionnelles :
- Ajout, suppression, mise à jour d'un artiste
- Ajout d'un album à un artiste, association automatique album ↔ artiste
- Ajout et suppression d'une track dans un album, association automatique track ↔ album
- Gestion des cas d'erreur (éléments manquants, doublons, valeurs nulles)

### API (WebApi)
- Authentification auprès de l'API Spotify (Client Credentials Flow), avec mise en cache du token
- Recherche combinant artistes, albums et morceaux en une seule liste de résultats
- Détail d'un artiste avec ses albums (pagination "charger plus" pour limiter les appels externes)
- Détail d'un album avec toutes ses pistes
- Détail d'une piste avec son extrait audio, avec repli automatique sur une autre source si
  l'extrait n'est pas disponible directement
- Configuration CORS pour autoriser les appels depuis le client web
- Persistance en base de données SQL Server (via Entity Framework Core) des artistes, albums et
  morceaux consultés, pour limiter les appels à l'API externe sur un contenu déjà visité
- Création de blind tests : sélection d'un ensemble de morceaux (par nom, catégorie/thème), pour
  constituer un pool de questions réutilisable sur plusieurs parties
- Lancement et déroulement d'un blind test : tirage aléatoire de questions à partir d'un blind
  test choisi, validation des réponses avec tolérance orthographique, suivi de la progression

### Client web (JavaScript)
- Champ de recherche avec temporisation (debounce) avant l'appel à l'API
- Résultats affichés sous forme de cartes cliquables, avec navigation croisée : depuis un artiste
  on accède à ses albums, depuis un album à ses morceaux et à son artiste, depuis un morceau on
  peut remonter à son album
- Lecteur audio dédié pour un morceau : pochette animée pendant la lecture, barre de progression
  interactive, navigation au morceau précédent/suivant, réglage du volume
- Gestion des différents états : chargement, aucun résultat, erreur, recherche vide
- Interface responsive
- Écran de création d'un blind test : recherche de morceaux dédiée, sélection par cases à cocher
  synchronisée entre les résultats de recherche et la liste de sélection en cours
- Mode jeu : liste des blind tests disponibles, lecture automatique de l'extrait avec visualiseur
  audio, validation des réponses avec retour visuel immédiat, écran de résultats final avec score
  et récapitulatif des morceaux joués

## Structure du projet
```text
BlindTest
│
├── Entities
│   ├── Artist.cs
│   ├── Album.cs
│   ├── Track.cs
│   └── ArtistManager.cs
│
├── WebApi
│   ├── Services/
│   │   ├── SpotifyAuthService.cs
│   │   └── SpotifyApiService.cs
│   ├── Dtos/
│   └── Controllers/
│       ├── SearchController.cs
│       ├── ArtistsController.cs
│       ├── AlbumsController.cs
│       └── TracksController.cs
│
└── client-js
    ├── index.html
    ├── style.css
    └── script.js
```

## Technologies utilisées
- C# / .NET
- ASP.NET Core Web API
- HTML / CSS / JavaScript
- API Spotify / API Deezer
- Git

## Remarques
Les tests unitaires utilisés pendant le développement proviennent du cadre pédagogique DIIAGE et
ne sont volontairement pas présents dans ce dépôt.
