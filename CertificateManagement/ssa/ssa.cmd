openssl req -new -newkey rsa:2048 -keyout ssa.key -sha256 -nodes -out ssa.csr -config ssa.cnf
openssl req -in ssa.csr -noout -text
openssl x509 -req -days 1826 -in ssa.csr -out ssa.pem -extfile ssa.ext -signkey ssa.key
openssl pkcs12 -inkey ssa.key -in ssa.pem -export -out ssa.pfx
openssl pkcs12 -in ssa.pfx -noout -info
