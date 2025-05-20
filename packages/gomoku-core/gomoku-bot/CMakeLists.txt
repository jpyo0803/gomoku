cmake_minimum_required(VERSION 3.10)
project(gomoku_server)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# 라이브러리 소스
set(GOMOKU_LIB_SRC
    board.cpp
    gomoku_bot.cpp
    gomoku_bot_strategy.cpp
)

set(GOMOKU_LIB_HEADERS
    board.h
    gomoku_bot.h
    gomoku_bot_strategy.h
    igomoku_bot_strategy.h
)

# gomoku_core 라이브러리 정의
add_library(gomoku_core STATIC ${GOMOKU_LIB_SRC} ${GOMOKU_LIB_HEADERS})

# server 실행 파일 정의
add_executable(gomoku_server server.cpp)

# server가 gomoku_core를 링크하도록 설정
target_link_libraries(gomoku_server PRIVATE gomoku_core)
