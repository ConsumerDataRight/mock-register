openssl genrsa -aes256 -passout pass:#M0ckCDRCA# -out ca.pass.key 4096
openssl rsa -passin pass:#M0ckCDRCA# -in ca.pass.key -out ca.key
rm ca.pass.key

openssl req -new -x509 -days 3650 -key ca.key -out ca.pem
openssl x509 -text -noout -in ca.pem

openssl pkcs12 -inkey ca.key -in ca.pem -export -out ca.pfx
openssl pkcs12 -in ca.pfx -noout -info
