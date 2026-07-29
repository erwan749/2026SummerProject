# BlindTest - Implémentation personnelle

## Contexte
Projet réalisé dans le cadre du projet d'été des DIIAGE.
Ce dépôt contient uniquement mon implémentation personnelle réalisée pendant ce projet.
Les énoncés, ressources pédagogiques et tests fournis par l'école ne sont pas inclus dans ce dépôt.

## Description du projet
Ce projet consiste en la mise en place d'une application de blind test musical : gestion d'entités
représentant des artistes, des albums et des morceaux, et une API permettant de rechercher ce
contenu via un fournisseur de données musicales externe (Spotify).

## Fonctionnalités implémentées

### Modèle de données (Entities)
Gestion en mémoire des artistes, albums et morceaux, avec relations bidirectionnelles entre eux :
- Ajout, suppression, mise à jour d'un artiste
- Recherche d'un artiste par identifiant externe
- Ajout d'un album à un artiste, association automatique album ↔ artiste
- Ajout et suppression d'une track dans un album, association automatique track ↔ album
- Gestion des cas d'erreur (éléments manquants, doublons, valeurs nulles)

### API (WebApi)
- Authentification auprès de l'API Spotify (Client Credentials Flow), avec mise en cache du token
  d'accès pour éviter des demandes répétées
- Endpoint de recherche combinant artistes, albums et morceaux en une seule liste de résultats,
  avec image, sous-titre et identifiant d'artiste associé pour chaque élément

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
└── WebApi
    ├── Services/
    │   ├── SpotifyAuthService.cs
    │   └── SpotifyApiService.cs
    ├── Dtos/
    └── Controllers/
        └── SearchController.cs
```

## Technologies utilisées
- C# / .NET
- ASP.NET Core Web API
- API Spotify
- Git

## Remarques
Les tests unitaires utilisés pendant le développement proviennent du cadre pédagogique DIIAGE et
ne sont volontairement pas présents dans ce dépôt.
