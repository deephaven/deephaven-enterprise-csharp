# builds a docker image that will build and test the DeephavenOpenAPI project

if [ "$#" -lt 2 ]; then
        echo "Usage: docker-build.sh Release|Debug <version>"
        echo "<version> must take the form X.Y.Z.V (all numeric)"
        exit 1
fi

docker build --build-arg PROJECT_NAME=DeephavenOpenAPI --build-arg CONFIGURATION=$1 --build-arg VERSION=$2 .
