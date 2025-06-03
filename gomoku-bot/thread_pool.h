#pragma once
#include <atomic>
#include <condition_variable>
#include <functional>
#include <future>
#include <mutex>
#include <queue>
#include <thread>
#include <vector>

class ThreadPool {
 public:
  ThreadPool(int num_threads);
  ~ThreadPool();

  // 모든 호출 가능한 객체를 받아 실행할 수 있도록한 일반화 템플릿 함수
  template <class F, class... Args>
  auto Enqueue(F&& f, Args&&... args) -> std::future<typename std::invoke_result<F, Args...>::type>;

 private:
  std::vector<std::thread> workers_;
  std::queue<std::function<void()>> tasks_;

  std::mutex queue_mutex_;
  std::condition_variable condition_;
  std::atomic<bool> stop_;
};

#include "thread_pool.inl"  // 템플릿 구현 분리