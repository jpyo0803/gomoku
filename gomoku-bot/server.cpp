#include <iostream>
#include <mutex>
#include "gomoku_bot.h"
#include "gomoku_bot_strategy.h"
#include "httplib.h"
#include "json.hpp"
#include "thread_pool.h"
#include "uuid.h"

using json = nlohmann::json;
using namespace std;

namespace {
constexpr int kPortNumber = 8080;  // 서버가 리스닝할 포트 번호

constexpr int kNumThreads = 4;
}  // namespace

class GomokuBotServer {
 public:
  GomokuBotServer() : pool_(kNumThreads) {
    svr_.Post("/solve", [this](const httplib::Request& req, httplib::Response& res) {
      try {
        json body = json::parse(req.body);
        std::string flat_board = body["board"];
        std::string task_id = GenerateUUID();  // Unique task id 생성후 client에게 식별자로서 제공

        // thread pool에 request 테스크 추가
        pool_.Enqueue([flat_board, task_id, this]() {
          try {
            gomoku::Board board(flat_board);
            gomoku::GomokuBot bot;
            bot.set_strategy(std::make_unique<gomoku::MinimaxWithAlphaBetaPruning>());
            auto [x, y] = bot.Solve(board, 4);

            std::cout << "Task ID: " << task_id << ", Move: (" << x << ", " << y << ")"
                      << std::endl;
            std::lock_guard<std::mutex> lock(task_mutex_);
            task_results_[task_id] = {true, {x, y}};
          } catch (...) {
            // ...
          }
        });

        json resp = {{"task_id", task_id}};
        res.set_content(resp.dump(), "application/json");

      } catch (const std::exception& e) {
        res.status = 400;
        res.set_content(json({{"error", e.what()}}).dump(), "application/json");
      }
    });

    svr_.Get(R"(/result/([-\w]+))", [this](const httplib::Request& req, httplib::Response& res) {
      std::string task_id =
          req.matches[1];  // Client쪽에서는 주기적으로 결과 확인시 task_id도 같이 보냄
      std::lock_guard<std::mutex> lock(task_mutex_);

      // std::cout << "Checking result for task ID: " << task_id << std::endl;
      if (task_results_.count(task_id)) {
        auto [done, result] = task_results_[task_id];
        if (done) {
          res.set_content(
              json({{"status", "done"}, {"x", result.first}, {"y", result.second}}).dump(),
              "application/json");
          // task 삭제
          task_results_.erase(task_id);
        } else {
          res.set_content(json({{"status", "pending"}}).dump(), "application/json");
        }
      } else {
        res.status = 404;
        res.set_content(json({{"error", "task not found"}}).dump(), "application/json");
      }
    });
  }

  void Run() {
    std::cout << "Server is running on http://localhost:" << kPortNumber << std::endl;
    svr_.listen("0.0.0.0", kPortNumber);
  };

 private:
  httplib::Server svr_;

  ThreadPool pool_;

  std::unordered_map<std::string, std::pair<bool, std::pair<int, int>>> task_results_;
  std::mutex task_mutex_;
};

int main() {
  GomokuBotServer bot_server;
  bot_server.Run();
  return 0;
}