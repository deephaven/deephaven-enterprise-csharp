#rm -rf ./docfx_project/src/*
#mkdir -p ./docfx_project/src/Deephaven/OpenAPI/Client/
#cp -R ./DeephavenOpenAPI/Deephaven/OpenAPI/Client/* ./docfx_project/src/Deephaven/OpenAPI/Client/
#cp -R ./DeephavenOpenAPI ./docfx_project/src/
#cp -R ./Core ./docfx_project/src/
#cp -R ./SharedCustom ./docfx_project/src/ 
#cp -R # generate docs
docfx docfx.json
