    docker build -t savethechickendevelopmentpostgres .

    docker run -d --name savethechickendev-postgres -p 5432:5432 savethechickendevelopmentpostgres
