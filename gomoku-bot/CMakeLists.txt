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

# gomoku_core 라이브러리 정의
add_library(gomoku_core STATIC ${GOMOKU_LIB_SRC} ${GOMOKU_LIB_HEADERS})

# server 실행 파일 정의
add_executable(gomoku_server server.cpp)

target_include_directories(gomoku_server PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/external/websocketpp
)

# server가 gomoku_core를 링크하도록 설정
target_link_libraries(gomoku_server PRIVATE gomoku_core)
