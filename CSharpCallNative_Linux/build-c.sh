gcc -std=gnu11 -fPIC -c MySo.c
gcc -shared MySo.o -o libMySo.so
rm MySo.o
