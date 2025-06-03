#include "thread_pool.h"

inline ThreadPool::ThreadPool(int num_threads) : stop_(false) {
  for (size_t i = 0; i < num_threads; ++i) {
    workers_.emplace_back([this]() {
      while (true) {
        std::function<void()> task;
        {
          std::unique_lock<std::mutex> lock(queue_mutex_);
          condition_.wait(lock, [this]() { return stop_ || !tasks_.empty(); });
          if (stop_ && tasks_.empty()) return;
          task = std::move(tasks_.front());
          tasks_.pop();
        }
        task();
      }
    });
  }
}

inline ThreadPool::~ThreadPool() {
  stop_ = true;
  condition_.notify_all();
  for (std::thread &worker : workers_) {
    if (worker.joinable()) worker.join();
  }
}

template<class F, class... Args>
auto ThreadPool::Enqueue(F&& f, Args&&... args)
    -> std::future<typename std::invoke_result<F, Args...>::type> {
  using return_type = typename std::invoke_result<F, Args...>::type;

  auto task = std::make_shared<std::packaged_task<return_type()>>(
      std::bind(std::forward<F>(f), std::forward<Args>(args)...));

  std::future<return_type> res = task->get_future();
  {
    std::lock_guard<std::mutex> lock(queue_mutex_);
    tasks_.emplace([task]() { (*task)(); });
  }
  condition_.notify_one();
  return res;
}